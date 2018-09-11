import { decorate, computed, action } from "mobx";

import { IGridView, IGridProps, IGridSelectors, IGridActions } from "./types";
import { CPR } from "../utils/canvas";

export class GridView implements IGridView {
  constructor(public selectors: IGridSelectors, public actions: IGridActions) {}

  public get contentWidth(): number {
    return 5000;
  }

  public get contentHeight(): number {
    return 5000;
  }

  public get width(): number {
    return this.selectors.width;
  }

  public get height(): number {
    return this.selectors.height;
  }

  public get innerWidth(): number {
    return this.selectors.innerWidth;
  }

  public get innerHeight(): number {
    return this.selectors.innerHeight;
  }

  public get canvasProps() {
    return {
      width: this.canvasWidthPX,
      height: this.canvasHeightPX,
      style: {
        width: this.canvasWidthCSS,
        height: this.canvasHeightCSS
      }
    };
  }

  public get canvasWidthPX() {
    return Math.ceil(this.innerWidth * CPR) || 0;
  }

  public get canvasHeightPX() {
    return Math.ceil(this.innerHeight * CPR) || 0;
  }

  public get canvasWidthCSS() {
    return Math.ceil(this.innerWidth * CPR) / CPR || 0;
  }

  public get canvasHeightCSS() {
    return Math.ceil(this.innerHeight * CPR) / CPR || 0;
  }

  public handleGridScroll(event: any): void {
    this.actions.handleScroll(event);
  }

  public handleGridKeyDown(event: any): void {
    this.actions.handleKeyDown(event);
  }

  public handleGridClick(event: any): void {
    throw new Error("Method not implemented.");
  }

  public handleResize(width: number, height: number) {
    this.actions.handleResize(width, height);
  }

  public refRoot(element: HTMLDivElement): void {
    this.actions.refRoot(element);
  }

  public refScroller(element: HTMLDivElement): void {
    this.actions.refScroller(element);
  }

  public refCanvas(element: HTMLCanvasElement): void {
    this.actions.refCanvas(element);
  }

  public componentDidMount(
    props: IGridProps,
    component: React.Component<IGridProps>
  ): void {
    const { width, height } = props;
    this.handleResize(width, height);
  }

  public componentDidUpdate(
    prevProps: IGridProps,
    props: IGridProps,
    component: React.Component<IGridProps>
  ): void {
    const { width, height } = props;
    this.handleResize(width, height);
  }

  public componentWillUnmount(): void {
    throw new Error("Method not implemented.");
  }
}

decorate(GridView, {
  canvasProps: computed,
  contentHeight: computed,
  contentWidth: computed,
  width: computed,
  height: computed,
  innerWidth: computed,
  innerHeight: computed,
  componentDidMount: action.bound,
  componentDidUpdate: action.bound,
  componentWillUnmount: action.bound,
  refCanvas: action.bound,
  refScroller: action.bound,
  refRoot: action.bound,
  handleResize: action.bound,
  handleGridClick: action.bound,
  handleGridKeyDown: action.bound,
  handleGridScroll: action.bound
});
