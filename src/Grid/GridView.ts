import { decorate, computed, action } from "mobx";

import { IGridView, IGridProps, IGridSelectors, IGridActions } from "./types";
import { CPR } from "../utils/canvas";

export class GridView implements IGridView {


  constructor(public selectors: IGridSelectors, public actions: IGridActions) {}

  @computed
  public get contentWidth(): number {
    return this.selectors.contentWidth;
  }
  
  @computed
  public get contentHeight(): number {
    return this.selectors.contentHeight;
  }

  @computed
  public get width(): number {
    return this.selectors.width;
  }

  @computed
  public get height(): number {
    return this.selectors.height;
  }

  @computed
  public get innerWidth(): number {
    return this.selectors.innerWidth;
  }

  @computed
  public get innerHeight(): number {
    return this.selectors.innerHeight;
  }

  @computed
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

  @computed
  public get canvasWidthPX() {
    return Math.ceil(this.innerWidth * CPR) || 0;
  }

  @computed
  public get canvasHeightPX() {
    return Math.ceil(this.innerHeight * CPR) || 0;
  }

  @computed
  public get canvasWidthCSS() {
    return Math.ceil(this.innerWidth * CPR) / CPR || 0;
  }

  @computed
  public get canvasHeightCSS() {
    return Math.ceil(this.innerHeight * CPR) / CPR || 0;
  }

  @computed
  public get fixedColumnCount(): number {
    return this.selectors.fixedColumnCount;
  }

  @computed
  public get movingColumnsTotalWidth(): number {
    return this.selectors.fixedColumnsTotalWidth;
  }

  @computed
  public get columnHeadersOffsetLeft(): number {
    return this.selectors.columnHeadersOffsetLeft;
  }

  @computed
  public get columnCount(): number {
    return this.selectors.columnCount;
  }

  public getColumnId(columnIndex: number): string {
    return this.selectors.getColumnId(columnIndex);
  }

  public getColumnLeft(columnIndex: number): number {
    return this.selectors.getColumnLeft(columnIndex);
  }

  public getColumnRight(columnIndex: number): number {
    return this.selectors.getColumnRight(columnIndex);
  }

  @action.bound
  public handleGridScroll(event: any): void {
    this.actions.handleScroll(event);
  }

  @action.bound
  public handleGridKeyDown(event: any): void {
    this.actions.handleKeyDown(event);
  }

  @action.bound
  public handleGridClick(event: any): void {
    this.actions.handleGridClick(event);
  }

  @action.bound
  public handleResize(width: number, height: number) {
    this.actions.handleResize(width, height);
  }

  @action.bound
  public refRoot(element: HTMLDivElement): void {
    this.actions.refRoot(element);
  }

  @action.bound
  public refScroller(element: HTMLDivElement): void {
    this.actions.refScroller(element);
  }

  @action.bound
  public refCanvas(element: HTMLCanvasElement): void {
    this.actions.refCanvas(element);
  }

  @action.bound
  public componentDidMount(
    props: IGridProps,
    component: React.Component<IGridProps>
  ): void {
    const { width, height } = props;
    this.handleResize(width, height);
    this.actions.componentDidMount(props);
  }

  @action.bound
  public componentDidUpdate(
    prevProps: IGridProps,
    props: IGridProps,
    component: React.Component<IGridProps>
  ): void {
    const { width, height } = props;
    this.handleResize(width, height);
    this.actions.componentDidUpdate(prevProps, props);
  }

  @action.bound
  public componentWillUnmount(): void {
    this.actions.componentWillUnmount();
  }
}

