import { ILoggedUserScope } from "./types/ILoggedUserScope";
import { ILoggedUser } from "../LoggedUser/types/ILoggedUser";
import { IAClearUserInfo } from "../LoggedUser/types/IAClearUserInfo";
import { IALogout } from "../LoggedUser/types/IALogout";
import { IAOnSubmitLogin } from "../LoggedUser/types/IAOnSubmitLogin";
import { LoggedUser } from "../LoggedUser/LoggedUser";
import { AOnSubmitLogin } from "../LoggedUser/AOnSubmitLogin";
import { AClearUserInfo } from "../LoggedUser/AClearUserInfo";
import { ML } from "../utils/types";
import { IMainMenu } from "../MainMenu/types";
import { IApplicationMachine } from "../Application/types/IApplicationMachine";
import { IApi } from "../Api/IApi";
import { ALogout } from "../LoggedUser/ALogout";

export class LoggedUserScope implements ILoggedUserScope {
  constructor(
    public P: {
      mainMenu: ML<IMainMenu>;
      appMachine: ML<IApplicationMachine>;
      api: ML<IApi>;
    }
  ) {}

  loggedUser: ILoggedUser = new LoggedUser({});
  aClearUserInfo: IAClearUserInfo = new AClearUserInfo({
    api: this.P.api,
    loggedUser: () => this.loggedUser,
    mainMenu: this.P.mainMenu
  });
  aLogout: IALogout = new ALogout({ appMachine: this.P.appMachine });
  aOnSubmitLogin: IAOnSubmitLogin = new AOnSubmitLogin({
    appMachine: this.P.appMachine
  });
}
