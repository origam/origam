import { IMainViewsScope } from "./IMainViewsScope";
import { ILoggedUserScope } from "./ILoggedUserScope";
import { IMainMenuScope } from "./IMainMenuScope";
import { IMainViewFactory } from "../../Screens/types";
import { IAStartTheMiracle } from "../../Application/types/IAStartTheMiracle";
import { IApplicationMachine } from "../../Application/types/IApplicationMachine";
import { IAnouncer } from "../../Application/types/IAnouncer";
import { IApi } from "../../Api/IApi";

export interface IApplicationScope {
  aStartTheMiracle: IAStartTheMiracle;
  appMachine: IApplicationMachine;
  anouncer: IAnouncer;
  api: IApi;
  mainViewsScope: IMainViewsScope;
  loggedUserScope: ILoggedUserScope;
  mainMenuScope: IMainMenuScope;
}
