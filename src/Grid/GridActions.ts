import { decorate, action } from "mobx";
import { IGridState, IGridSelectors, IGridActions } from "./types";

export class GridActions implements IGridActions {


  constructor(public state: IGridState, public selectors: IGridSelectors) {}

  public handleResize(width: number, height: number): void {
    this.state.setSize(width, height);
  }

  public refRoot(element: HTMLDivElement): void {
    this.state.setRefRoot(element);
  }

  public refScroller(element: HTMLDivElement): void {
    this.state.setRefScroller(element);
  }

  public refCanvas(element: HTMLCanvasElement): void {
    this.state.setRefCanvas(element);
  }

}

decorate(GridActions, {
  handleResize: action.bound,
  refRoot: action.bound,
  refScroller: action.bound,
  refCanvas: action.bound,
})