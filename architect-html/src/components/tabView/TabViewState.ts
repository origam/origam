import { observable } from "mobx";

export class TabViewState {
  @observable accessor activeTabIndex = 0;

  showModelTree() {
    this.activeTabIndex = 1;
  }
}