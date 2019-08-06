import {
  IOpenedScreen,
  IOpenedScreenData,
  IDialogInfo
} from "./types/IOpenedScreen";
import { observable } from "mobx";
import { IFormScreen } from "./types/IFormScreen";
import { IMainMenuItemType } from "./types/IMainMenu";

export class DialogInfo implements IDialogInfo {
  constructor(public width: number, public height: number) {}
}

export class OpenedScreen implements IOpenedScreen {
  $type_IOpenedScreen: 1 = 1;

  constructor(data: IOpenedScreenData) {
    Object.assign(this, data);
    this.content.parent = this;
  }

  @observable isActive = false;
  menuItemId: string = "";
  menuItemType: IMainMenuItemType = null as any;
  order: number = 0;
  title: string = "";
  @observable content: IFormScreen = null as any;

  setActive(state: boolean): void {
    this.isActive = state;
  }

  setContent(screen: IFormScreen): void {
    this.content = screen;
    screen.parent = this;
  }

  parent?: any;
}
