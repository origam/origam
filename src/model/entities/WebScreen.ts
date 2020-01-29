import { IWebScreen } from "./types/IWebScreen";
import { IOpenedScreen } from "./types/IOpenedScreen";
import { observable, action } from "mobx";
import { IFormScreenEnvelope } from "./types/IFormScreen";
import { IMainMenuItemType } from "./types/IMainMenu";

export class WebScreen implements IWebScreen, IOpenedScreen {
  $type_IOpenedScreen: 1 = 1;
  $type_IWebScreen: 1 = 1;

  constructor(public title: string, public screenUrl: string) {}

  @observable isActive = false;
  isDialog = false;

  @action.bound
  setActive(state: boolean): void {
    this.isActive = state;
  }

  setContent(screen: IFormScreenEnvelope): void {}

  parent?: any;
  menuItemId: string = "";
  menuItemType: IMainMenuItemType = null as any;
  dontReque = undefined;
  order: number = 0;
  dontRequestData = false;
  dialogInfo = undefined;
  content: IFormScreenEnvelope = null as any;
  parameters: { [key: string]: any } = {};
}
