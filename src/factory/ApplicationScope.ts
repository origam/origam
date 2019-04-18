import { IApplicationScope } from "./types/IApplicationScope";
import { IMainViewsScope } from "./types/IMainViewsScope";
import { ILoggedUserScope } from "./types/ILoggedUserScope";
import { IMainMenuScope } from "./types/IMainMenuScope";
import { MainMenuScope } from "./MainMenuScope";
import { AStartTheMiracle } from "../Application/AStartTheMiracle";
import { ApplicationMachine } from "../Application/ApplicationMachine";
import { MainViewsScope } from "./MainViewsScope";
import { Anouncer } from "../Application/Anouncer";
import { LoggedUserScope } from "./LoggedUserScope";
import { OrigamAPI } from "../Api/OrigamAPI";

export class ApplicationScope implements IApplicationScope {
  mainMenuScope: IMainMenuScope = new MainMenuScope({
    aOpenView: () => this.mainViewsScope.aOpenView,
    aActivateOrOpenView: () => this.mainViewsScope.aActivateOrOpenView
  });

  loggedUserScope: ILoggedUserScope = new LoggedUserScope({
    mainMenu: () => this.mainMenuScope.mainMenu,
    appMachine: () => this.appMachine,
    api: () => this.api
  });

  mainViewsScope: IMainViewsScope = new MainViewsScope({ api: () => this.api });

  aStartTheMiracle = new AStartTheMiracle({
    appMachine: () => this.appMachine
  });

  appMachine = new ApplicationMachine({
    anouncer: () => this.anouncer,
    api: () => this.api,
    mainMenu: () => this.mainMenuScope.mainMenu,
    aClearUserInfo: () => this.loggedUserScope.aClearUserInfo,
    loggedUser: () => this.loggedUserScope.loggedUser,
    mainMenuFactory: () => this.mainMenuScope.mainMenuFactory
  });

  anouncer = new Anouncer({});

  api = new OrigamAPI();
}
