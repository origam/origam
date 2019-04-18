import { ILoggedUser } from "../../LoggedUser/types/ILoggedUser";
import { IAClearUserInfo } from "../../LoggedUser/types/IAClearUserInfo";
import { IALogout } from "../../LoggedUser/types/IALogout";
import { IAOnSubmitLogin } from "../../LoggedUser/types/IAOnSubmitLogin";

export interface ILoggedUserScope {
  loggedUser: ILoggedUser;
  aClearUserInfo: IAClearUserInfo;
  aLogout: IALogout;
  aOnSubmitLogin: IAOnSubmitLogin;
}
