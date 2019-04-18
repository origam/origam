import { observable, action } from "mobx";
import { ILoggedUser } from "./types/ILoggedUser";

export class LoggedUser implements ILoggedUser {
  constructor(public P: {}) {}

  @observable userName: string = "";

  @action.bound setUserName(userName: string) {
    this.userName = userName;
  }

  @action.bound resetUserName() {
    this.userName = "";
  }
}
