import { action, flow, observable, createAtom } from "mobx";
import { Machine, interpret, Interpreter, State } from "xstate";

import {
  IApplicationLifecycle,
  IApplicationPage
} from "./types/IApplicationLifecycle";
import { getApi } from "../selectors/getApi";
import { getApplication } from "../selectors/getApplication";
import { createWorkbench } from "../factories/createWorkbench";

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
  $type_IApplicationLifecycle: 1 = 1;

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
    } catch (error) {
      console.error(error);
      this.setLoginPageMessage("Login failed.");
      this.interpreter.send(loginFailed);
    }
  }

  *performLogout(args: any) {
    try {
      const api = getApi(this);
      const application = getApplication(this);
      window.sessionStorage.removeItem("origamAuthToken");

      application.resetWorkbench();
      try {
        yield api.logout();
      } finally {
        api.resetAccessToken();
      }
      this.interpreter.send(logoutSuccessful);
      return null;
    } catch (error) {
      console.error(error)
      this.interpreter.send({ type: logoutFailed, error });
    }
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

  parent?: any;
}
