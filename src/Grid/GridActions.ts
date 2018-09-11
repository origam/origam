import { decorate, action } from "mobx";
import { IGridState, IGridSelectors, IGridActions, IGridSetup } from "./types";
// import { getElementPosition } from "../utils/elements";

class AnimationFrameScheduler {
  private scheduled: boolean = false;

  public schedule(fn: () => void) {
    if (!this.scheduled) {
      this.scheduled = true;
      requestAnimationFrame(() => {
        this.scheduled = false;
        fn();
      });
    }
  }
}

decorate(AnimationFrameScheduler, {
  schedule: action.bound
});

export class GridActions implements IGridActions {
  constructor(
    public state: IGridState,
    public selectors: IGridSelectors,
    public setup: IGridSetup
  ) {}

  get fixedColumnCount(): number {
    return this.selectors.fixedColumnCount;
  }

  public handleResize(width: number, height: number): void {
    this.state.setSize(width, height);
  }

  public handleScroll(event: any): void {
    this.state.setScroll(event.target.scrollTop, event.target.scrollLeft);
  }

  public handleKeyDown(event: any): void {
    throw new Error("Method not implemented.");
  }

  public refRoot(element: HTMLDivElement): void {
    this.state.setRefRoot(element);
  }

  public refScroller(element: HTMLDivElement): void {
    this.state.setRefScroller(element);
  }

  public refCanvas(element: HTMLCanvasElement): void {
    this.state.setRefCanvas(element);
    if (element) {
      this.state.setCanvasContext(element.getContext("2d")!);
    } else {
      this.state.setCanvasContext(null);
    }
  }
}

decorate(GridActions, {
  handleResize: action.bound,
  refRoot: action.bound,
  refScroller: action.bound,
  refCanvas: action.bound
});
