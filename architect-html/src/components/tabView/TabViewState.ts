import { observable } from "mobx";

export class TabViewState {
  @observable accessor activeTabIndex = 0;
}