import { makeObservable, observable } from "mobx";

export class TabViewState {
  constructor() {
    makeObservable(this, {
      activeTabIndex: observable,
    })
  }

  activeTabIndex = 0;
}