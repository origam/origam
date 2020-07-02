import {IScrollState} from "./types";
import {action, observable} from "mobx";

export class SimpleScrollState implements IScrollState {
  scrollToFunction: ((coords: { scrollLeft?: number; scrollTop?: number }) => void) | undefined;
  constructor(scrollTop: number, scrollLeft: number) {
    this.scrollTop = scrollTop;
    this.scrollLeft = scrollLeft;
  }

  scrollTo(coords: { scrollLeft?: number; scrollTop?: number }){
    if(this.scrollToFunction){
      this.scrollToFunction(coords);
    }
  }
  
  @observable scrollTop = 0;
  @observable scrollLeft = 0;

  @action.bound
  setScrollOffset(event: any, scrollTop: number, scrollLeft: number): void {
    // console.log("scroll event: ", scrollTop, scrollLeft);
    this.scrollTop = scrollTop;
    this.scrollLeft = scrollLeft;
  }
}
