import { ILoggedUser } from "./types/ILoggedUser";
import { IAClearUserInfo } from "./types/IAClearUserInfo";
import { action } from "mobx";
import { IApi } from "../Api/IApi";
import { ML } from "../utils/types";
import { unpack } from "../utils/objects";
import { IMainMenu } from "../MainMenu/types";

export class AClearUserInfo implements IAClearUserInfo {
  constructor(
    public P: {
      api: ML<IApi>;
      loggedUser: ML<ILoggedUser>;
      mainMenu: ML<IMainMenu>;
    }
  ) {}

  @action.bound do() {
    this.api.resetAccessToken();
    this.loggedUser.resetUserName();
    this.mainMenu.resetItems();
    window.sessionStorage.removeItem("origamAccessToken");
  }

  get api() {
    return unpack(this.P.api);
  }

  get loggedUser() {
    return unpack(this.P.loggedUser);
  }

  get mainMenu() {
    return unpack(this.P.mainMenu);
  }
}
