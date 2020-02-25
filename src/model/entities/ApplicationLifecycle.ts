import { action, computed, observable } from "mobx";
import { createWorkbench } from "../factories/createWorkbench";
import { getApi } from "../selectors/getApi";
import { getApplication } from "../selectors/getApplication";
import { IApplicationLifecycle, IApplicationPage } from "./types/IApplicationLifecycle";
import { stopWorkQueues } from "model/actions/WorkQueues/stopWorkQueues";
import { stopAllFormsAutorefresh } from "model/actions/Workbench/stopAllFormsAutorefresh";
import { userManager } from "oauth";

export class ApplicationLifecycle implements IApplicationLifecycle {
  $type_IApplicationLifecycle: 1 = 1;

  constructor() {}

  @observable loginPageMessage?: string | undefined;

  @observable displayedPage = IApplicationPage.Login;

  @observable inFlow = 0;
  @computed get isWorking() {
    return this.inFlow > 0;
  }

  get shownPage(): IApplicationPage {
    return this.displayedPage;
  }

  *onLoginFormSubmit(args: { event: any; userName: string; password: string }) {
    try {
      this.inFlow++;
      args.event.preventDefault();
      yield* this.performLogin(args);
    } finally {
      this.inFlow--;
    }
  }

  *onSignOutClick(args: { event: any }) {
    yield* this.performLogout();
  }

  *run() {
    yield* this.reuseAuthToken();
  }

  *performLogin(args: { userName: string; password: string }) {
    try {
      const api = getApi(this);
      const token = yield api.login({
        UserName: args.userName,
        Password: args.password
      });
      yield* this.anounceAuthToken(token);
    } catch (error) {
      console.error(error);
      // TODO: Distinguish between connection error and bad credentials etc.
      this.setLoginPageMessage("Login failed.");
      throw error;
    }
  }

  *performLogout() {
    const api = getApi(this);
    const application = getApplication(this);
    window.sessionStorage.removeItem("origamAuthToken");
    for (let sessionStorageKey of Object.keys(window.sessionStorage)) {
      if (sessionStorageKey.startsWith("oidc.user")) {
        // That is an oauth session...
        api.resetAccessToken();
        userManager.signoutRedirect();
        return;
      }
    }
    try {
      yield* stopAllFormsAutorefresh(application.workbench!)();
      yield* stopWorkQueues(application.workbench!)();
      application.resetWorkbench();
      try {
        yield api.logout();
      } finally {
        api.resetAccessToken();
      }
      return null;
    } finally {
      this.displayedPage = IApplicationPage.Login;
    }
  }

  *reuseAuthToken() {
    const token = window.sessionStorage.getItem("origamAuthToken");
    if (token) {
      yield* this.anounceAuthToken(token);
    }
  }

  *anounceAuthToken(token: string) {
    const api = getApi(this);
    const application = getApplication(this);
    window.sessionStorage.setItem("origamAuthToken", token);
    api.setAccessToken(token);
    const workbench = createWorkbench();
    application.setWorkbench(workbench);
    this.displayedPage = IApplicationPage.Workbench;
    yield* workbench.run();
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
