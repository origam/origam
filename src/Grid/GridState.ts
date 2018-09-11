import { IGridState } from "./types";
import { decorate, observable, action } from "mobx";

export class GridState implements IGridState {


  public width: number = 0;
  public height: number = 0;
  public elmRoot: HTMLDivElement | null = null;
  public elmScroller: HTMLDivElement | null = null;
  public elmCanvas: HTMLCanvasElement | null = null;

  public setSize(width: number, height: number): void {
    this.width = width;
    this.height = height;
  }

  public setRefRoot(element: HTMLDivElement): void {
    this.elmRoot = element;
  }

  public setRefScroller(element: HTMLDivElement): void {
    this.elmScroller = element;
  }

  public setRefCanvas(element: HTMLCanvasElement): void {
    this.elmCanvas = element;
  }
}

decorate(GridState, {
  width: observable,
  height: observable,
  elmRoot: observable.ref,
  elmScroller: observable.ref,
  elmCanvas: observable.ref,

  setSize: action.bound,
  setRefRoot: action.bound,
  setRefCanvas: action.bound,
  setRefScroller: action.bound,
});
