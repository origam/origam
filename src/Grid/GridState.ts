import { IGridState } from "./types";
import { decorate, observable, action } from "mobx";

export class GridState implements IGridState {

  public width: number = 0;
  public height: number = 0;
  public scrollTop: number = 0;
  public scrollLeft: number = 0;
  public component: React.Component | null = null;
  public elmRoot: HTMLDivElement | null = null;
  public elmScroller: HTMLDivElement | null = null;
  public elmCanvas: HTMLCanvasElement | null = null;
  public canvasContext: CanvasRenderingContext2D | null = null;

  public setSize(width: number, height: number): void {
    this.width = width;
    this.height = height;
  }

  public setScroll(scrollTop: number, scrollLeft: number): void {
    this.scrollTop = scrollTop;
    this.scrollLeft = scrollLeft;
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

  public setCanvasContext(context: CanvasRenderingContext2D | null): void {
    this.canvasContext = context;
  }

  public setComponent(component: React.Component): void {
    this.component = component;
  }
}

decorate(GridState, {
  width: observable,
  height: observable,
  scrollTop: observable,
  scrollLeft: observable,
  elmRoot: observable.ref,
  elmScroller: observable.ref,
  elmCanvas: observable.ref,
  component: observable.ref,

  setSize: action.bound,
  setRefRoot: action.bound,
  setRefScroller: action.bound,
  setRefCanvas: action.bound,
  setCanvasContext: action.bound,
  setComponent: action.bound,
});
