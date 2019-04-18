import { IMainMenuScope } from "./types/IMainMenuScope";
import { IMainMenuFactory, IMainMenu, IAOnItemClick } from "../MainMenu/types";
import { MainMenuFactory } from "../MainMenu/factory";
import { AOnItemClick } from "../MainMenu/AOnItemClick";
import { IAOpenView, IAActivateOrOpenView } from '../Screens/types';
import { ML } from "../utils/types";
import { unpack } from "../utils/objects";
import { MainMenu } from "../MainMenu/MainMenu";


export class MainMenuScope implements IMainMenuScope {
  constructor(public P:{
    aOpenView: ML<IAOpenView>;
    aActivateOrOpenView: ML<IAActivateOrOpenView>;
  }) {}

  mainMenuFactory: IMainMenuFactory = new MainMenuFactory({
    aOnItemClick: () => this.aOnItemClick
  });
  mainMenu: IMainMenu = new MainMenu({});
  aOnItemClick: IAOnItemClick = new AOnItemClick({
    aOpenView: () => this.aOpenView,
    aActivateOrOpenView: () => this.aActivateOrOpenView
  });

  get aOpenView() {
    return unpack(this.P.aOpenView);
  }

  get aActivateOrOpenView() {
    return unpack(this.P.aActivateOrOpenView);
  }
}