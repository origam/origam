import { action, computed, observable } from "mobx";
import { createWorkbench } from "../factories/createWorkbench";
import { getApi } from "../selectors/getApi";
import { getApplication } from "../selectors/getApplication";
import { IApplicationLifecycle, IApplicationPage } from "./types/IApplicationLifecycle";

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
    yield* this.performLogout(args);
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
      this.setLoginPageMessage("Login failed.");
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

      return null;
    } catch (error) {
      // TODO: Handle error
      console.error(error);
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
