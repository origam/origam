import { IOpenedScreen, IOpenedScreenData } from "./types/IOpenedScreen";
import { observable } from "mobx";

export class OpenedScreen implements IOpenedScreen {
  constructor(data: IOpenedScreenData) {
    Object.assign(this, data);
  }

  $type: "CLoadedOpenedScreen" = "CLoadedOpenedScreen";

  @observable isActive = false;
  menuItemId: string = "";
  order: number = 0;
  title: string = "";

  setActive(state: boolean): void {
    this.isActive = state;
  }

  parent?: any;
}
