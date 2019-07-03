import { action, flow, observable, createAtom } from "mobx";
import { Machine, interpret, Interpreter, State } from "xstate";

import {
  IApplicationLifecycle,
  IApplicationPage
} from "./types/IApplicationLifecycle";
import { getApi } from "./selectors/getApi";
import { getApplication } from "./selectors/getApplication";
import { createWorkbench } from "./factories/createWorkbench";

const loginFormSubmit = "loginFormSubmit";
const loginSuccessful = "loginSuccessful";
const loginFailed = "loginFailed";
const logout = "logout";
const logoutSuccessful = "logoutSuccessful";
const logoutFailed = "logoutFailed";

const sLoginPage = "sLoginPage";
const sPerformLogin = "sPerformLogin";
const sWorkbenchPage = "sWorkbenchPage";
const sPerformLogout = "sPerformLogout";

const Login = "Login";
const Workbench = "Workbench";

export class ApplicationLifecycle implements IApplicationLifecycle {
  parent?: any;

  constructor() {}

  @observable loginPageMessage?: string | undefined;

  machine = Machine(
    {
      initial: sLoginPage,
      states: {
        [sLoginPage]: {
          on: {
            [loginFormSubmit]: sPerformLogin
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

  *performLogin(args: { userName: string; password: string }) {
    try {
      const api = getApi(this);
      const application = getApplication(this);
      const token = yield api.login({
        UserName: args.userName,
        Password: args.password
      });
      api.setAccessToken(token);

      application.setWorkbench(createWorkbench());
      this.interpreter.send(loginSuccessful);
    } catch (e) {
      this.setLoginPageMessage("Login failed.");
      this.interpreter.send(loginFailed);
    }
  }

  *performLogout(args: any) {
    const api = getApi(this);
    const application = getApplication(this);
    api.resetAccessToken();
    application.resetWorkbench();
    this.interpreter.send(logoutSuccessful);
    return null;
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
