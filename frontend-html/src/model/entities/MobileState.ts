import { observable } from "mobx";
import { MainPageContents } from "gui/connections/MobileComponents/MobileMain";

export class MobileState {
  @observable
  tabsDroppedDown: boolean = false;

  @observable
  mainPageContents = MainPageContents.Screen
}