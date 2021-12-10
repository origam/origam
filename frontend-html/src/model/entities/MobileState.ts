import { IMobileState } from "model/entities/types/IMobileState";
import { observable } from "mobx";

export class MobileState implements IMobileState {
  @observable
  showMenu: boolean = false;

  @observable
  tabsDroppedDown: boolean = false;
}