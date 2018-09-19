import { IGridState, IGridProps, ICellRenderer } from "./types";
import { decorate, observable, action } from "mobx";

export class GridState implements IGridState {
  public cellRenderer: ICellRenderer;
  public width: number = 0;
  public height: number = 0;
  public scrollTop: number = 0;
  public scrollLeft: number = 0;
  public component: React.Component | null = null;
  public elmRoot: HTMLDivElement | null = null;
  public elmScroller: HTMLDivElement | null = null;
  public elmCanvas: HTMLCanvasElement | null = null;
  public canvasContext: CanvasRenderingContext2D | null = null;
  public onOutsideClick: ((event: any) => void) | undefined;
  public onScroll: ((event: any) => void) | undefined;
  public onKeyDown: ((event: any) => void) | undefined;

  public setSize(width: number, height: number): void {
    this.width = width;
    this.height = height;
  }

  public setScroll(scrollTop: number, scrollLeft: number): void {
    this.scrollTop = scrollTop;
    this.scrollLeft = scrollLeft;
  }

  public setScrollTop(scrollTop: number): void {
    this.scrollTop = scrollTop;
  }

  public setScrollLeft(scrollLeft: number): void {
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

  public setCellRenderer(cellRenderer: ICellRenderer): void {
    this.cellRenderer = cellRenderer;
  }

  public setOnOutsideClick(handler: (event: any) => void): void {
    this.onOutsideClick = handler;
  }

  public setOnScroll(handler: (event: any) => void): void {
    this.onScroll = handler;
  }

  public setOnKeyDown(handler: ((event: any) => void) | undefined): void {
    this.onKeyDown = handler;
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

  setSize: action.bound,
  setScroll: action.bound,
  setScrollTop: action.bound,
  setScrollLeft: action.bound,
  setRefRoot: action.bound,
  setRefScroller: action.bound,
  setRefCanvas: action.bound,
  setCanvasContext: action.bound
});
