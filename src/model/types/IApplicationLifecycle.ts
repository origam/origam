export enum IApplicationPage {
  Login = "Login",
  Workbench = "Workbench"
}

export interface IApplicationLifecycleData {}

export interface IApplicationLifecycle extends IApplicationLifecycleData {
  onLoginFormSubmit(args: {
    event: any;
    userName: string;
    password: string;
  }): void;
  onSignOutClick(args: {event: any}): void;

  parent?: any;
  shownPage: IApplicationPage;
  isWorking: boolean;

  loginPageMessage?: string;

  setLoginPageMessage(msg: string): void;
  resetLoginPageMessage(): void;
}
