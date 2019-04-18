import React from "react";

export interface IViewContainer {
  rvApp: IRVApp;
  rvLogin: IRVLogin;
  aSubmitLogin: IASubmitLogin;
}

export interface IRVApp {
  render(): React.ReactNode;
}

export interface IRVLogin {
  render(): React.ReactNode;
}

export interface IRVMain {
  render(): React.ReactNode;
}

export interface IRVMainMenu {
  render(): React.ReactNode;
}

export interface IRVMainViews {
  render(): React.ReactNode;
}

export interface IASubmitLogin {
  do(userName: string, password: string): void;
}
