import { action, flow, observable, createAtom } from "mobx";
import { Machine, interpret, Interpreter, State } from "xstate";

import {
  IApplicationLifecycle,
  IApplicationPage
} from "./types/IApplicationLifecycle";
import { getApi } from "./selectors/getApi";
import { getApplication } from "./selectors/getApplication";
import { createWorkbench } from "./factories/createWorkbench";
import { getOpenedScreens } from "./selectors/getOpenedScreens";
import { createOpenedScreen } from "./factories/createOpenedScreen";
import { IOpenedScreen } from "./types/IOpenedScreen";

const loginFormSubmit = "loginFormSubmit";
const loginSuccessful = "loginSuccessful";
const loginFailed = "loginFailed";
const logout = "logout";
const logoutSuccessful = "logoutSuccessful";
const logoutFailed = "logoutFailed";
const start = "start";

const sLoginPage = "sLoginPage";
const sPerformLogin = "sPerformLogin";
const sWorkbenchPage = "sWorkbenchPage";
const sPerformLogout = "sPerformLogout";
const sWaitForStart = "sWaitForStart";

const Login = "Login";
const Workbench = "Workbench";

export class ApplicationLifecycle implements IApplicationLifecycle {
  parent?: any;

  constructor() {}

  @observable loginPageMessage?: string | undefined;

  machine = Machine(
    {
      initial: sWaitForStart,
      states: {
        sWaitForStart: {
          on: {
            [start]: sLoginPage
          }
        },
        [sLoginPage]: {
          onEntry: "reuseAuthToken",
          on: {
            [loginFormSubmit]: sPerformLogin,
            [loginSuccessful]: sWorkbenchPage
          }
        },
        [sPerformLogin]: {
          invoke: { src: "performLogin" },
          on: {
            [loginSuccessful]: sWorkbenchPage,
            [loginFailed]: sLoginPage
          }
        },
        [sWorkbenchPage]: {
          on: {
            [logout]: sPerformLogout
          }
        },
        [sPerformLogout]: {
          invoke: { src: "performLogout" },
          on: {
            [logoutSuccessful]: sLoginPage,
            [logoutFailed]: sLoginPage
          }
        }
      }
    },
    {
      services: {
        performLogin: (ctx, event) => (send, onEvent) =>
          flow(this.performLogin.bind(this))(event as any),
        performLogout: (ctx, event) => (send, onEvent) =>
          flow(this.performLogout.bind(this))(event as any)
      },
      actions: {
        reuseAuthToken: (ctx, event) => this.reuseAuthToken()
      }
    }
  );

  stateAtom = createAtom("applicationLifecycleState");
  interpreter = interpret(this.machine)
    .onTransition((state, event) => {
      console.log("Application lifecycle:", state, event);
      this.stateAtom.reportChanged();
    })
    .start();

  get state() {
    this.stateAtom.reportObserved();
    return this.interpreter.state;
  }

  get shownPage(): IApplicationPage {
    switch (this.state.value) {
      case sWorkbenchPage:
      case sPerformLogout:
        return IApplicationPage.Workbench;
      default:
        return IApplicationPage.Login;
    }
  }

  get isWorking() {
    switch (this.state.value) {
      case sPerformLogin:
      case sPerformLogout:
        return true;
      default:
        return false;
    }
  }

  @action.bound
  onLoginFormSubmit(args: { event: any; userName: string; password: string }) {
    args.event.preventDefault();
    this.interpreter.send(loginFormSubmit, args);
  }

  @action.bound
  onSignOutClick(args: { event: any }): void {
    this.interpreter.send(logout, args);
  }

  @action.bound
  onMainMenuItemClick(args: { event: any; item: any }): void {
    const { type, id, label } = args.item.attributes;
    const { event } = args;
    switch (type) {
      case "FormReferenceMenuItem":
        {
          const openedScreens = getOpenedScreens(this);
          const existingItem = openedScreens.findLastExistingItem(id);
          if (!event.ctrlKey) {
            if (existingItem) {
              openedScreens.activateItem(id, existingItem.order);
            } else {
              const newScreen = createOpenedScreen(id, 0, label);
              openedScreens.pushItem(newScreen);
              openedScreens.activateItem(newScreen.menuItemId, newScreen.order);
            }
          } else {
            if (existingItem) {
              const newScreen = createOpenedScreen(
                id,
                existingItem.order + 1,
                label
              );
              openedScreens.pushItem(newScreen);
              openedScreens.activateItem(newScreen.menuItemId, newScreen.order);
            } else {
              const newScreen = createOpenedScreen(id, 0, label);
              openedScreens.pushItem(newScreen);
              openedScreens.activateItem(id, 0);
            }
          }
          console.log(openedScreens.items);
        }
        break;
      case "FormReferenceMenuItem_WithSelection":
        break;
    }
  }

  @action.bound
  onScreenTabHandleClick(event: any, openedScreen: IOpenedScreen) {
    const openedScreens = getOpenedScreens(this);
    openedScreens.activateItem(openedScreen.menuItemId, openedScreen.order);
  }

  @action.bound
  onScreenTabCloseClick(event: any, openedScreen: IOpenedScreen): void {
    event.stopPropagation();
    console.log(openedScreen);
    const openedScreens = getOpenedScreens(this);
    const closestScreen = openedScreens.findClosestItem(
      openedScreen.menuItemId,
      openedScreen.order
    );
    if (closestScreen) {
      openedScreens.activateItem(closestScreen.menuItemId, closestScreen.order);
    }
    openedScreens.deleteItem(openedScreen.menuItemId, openedScreen.order);
  }

  @action.bound
  run(): void {
    this.interpreter.send(start);
  }

  *performLogin(args: { userName: string; password: string }) {
    try {
      const api = getApi(this);
      const token = yield api.login({
        UserName: args.userName,
        Password: args.password
      });
      this.anounceAuthToken(token);
      this.interpreter.send(loginSuccessful);
    } catch (e) {
      this.setLoginPageMessage("Login failed.");
      this.interpreter.send(loginFailed);
      console.error(e);
    }
  }

  *performLogout(args: any) {
    const api = getApi(this);
    const application = getApplication(this);
    window.sessionStorage.removeItem("origamAuthToken");
    api.resetAccessToken();
    application.resetWorkbench();
    this.interpreter.send(logoutSuccessful);
    return null;
  }

  @action.bound reuseAuthToken() {
    const token = window.sessionStorage.getItem("origamAuthToken");
    if (token) {
      this.anounceAuthToken(token);
      this.interpreter.send(loginSuccessful);
    }
  }

  @action.bound anounceAuthToken(token: string) {
    const api = getApi(this);
    const application = getApplication(this);
    window.sessionStorage.setItem("origamAuthToken", token);
    api.setAccessToken(token);
    const workbench = createWorkbench();
    application.setWorkbench(workbench);
    workbench.run();
  }

  @action.bound
  setLoginPageMessage(msg: string): void {
    this.loginPageMessage = msg;
  }

  @action.bound
  resetLoginPageMessage(): void {
    this.loginPageMessage = undefined;
  }
}
