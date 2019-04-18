import { IScrollState } from "./types";
import { observable, action } from "mobx";

export class TableViewScrollState implements IScrollState {
  constructor(scrollTop: number, scrollLeft: number) {
    this.scrollTop = scrollTop;
    this.scrollLeft = scrollLeft;
  }
  @observable scrollTop = 0;
  @observable scrollLeft = 0;

  @action.bound
  setScrollOffset(event: any, scrollTop: number, scrollLeft: number): void {
    console.log("scroll event: ", scrollTop, scrollLeft);
    this.scrollTop = scrollTop;
    this.scrollLeft = scrollLeft;
  }
}