import { IGridState, IGridProps, ICellRenderer } from "./types";
import { decorate, observable, action } from "mobx";

export class GridState implements IGridState {

  @observable
  public cellRenderer: ICellRenderer;
  
  @observable 
  public width: number = 0;
  
  @observable 
  public height: number = 0;
  
  @observable 
  public scrollTop: number = 0;
  
  @observable 
  public scrollLeft: number = 0;
  
  @observable.ref
  public component: React.Component | null = null;
  
  @observable.ref
  public elmRoot: HTMLDivElement | null = null;
  
  @observable.ref
  public elmScroller: HTMLDivElement | null = null;

  @observable.ref
  public elmCanvas: HTMLCanvasElement | null = null;

  @observable.ref
  public canvasContext: CanvasRenderingContext2D | null = null;
  
  public onOutsideClick: ((event: any) => void) | undefined;
  public onScroll: ((event: any) => void) | undefined;
  public onKeyDown: ((event: any) => void) | undefined
  public onNoCellClick: ((event: any) => void) | undefined;

  @action.bound
  public setSize(width: number, height: number): void {
    this.width = width;
    this.height = height;
  }

  @action.bound
  public setScroll(scrollTop: number, scrollLeft: number): void {
    this.scrollTop = scrollTop;
    this.scrollLeft = scrollLeft;
  }

  @action.bound
  public setScrollTop(scrollTop: number): void {
    this.scrollTop = scrollTop;
  }

  @action.bound
  public setScrollLeft(scrollLeft: number): void {
    this.scrollLeft = scrollLeft;
  }

  @action.bound
  public setRefRoot(element: HTMLDivElement): void {
    this.elmRoot = element;
  }

  @action.bound
  public setRefScroller(element: HTMLDivElement): void {
    this.elmScroller = element;
  }

  @action.bound
  public setRefCanvas(element: HTMLCanvasElement): void {
    this.elmCanvas = element;
  }

  @action.bound
  public setCanvasContext(context: CanvasRenderingContext2D | null): void {
    this.canvasContext = context;
  }

  @action.bound
  public setCellRenderer(cellRenderer: ICellRenderer): void {
    this.cellRenderer = cellRenderer;
  }

  @action.bound
  public setOnOutsideClick(handler: (event: any) => void): void {
    this.onOutsideClick = handler;
  }

  @action.bound
  public setOnNoCellClick(handler: ((event: any) => void) | undefined): void {
    this.onNoCellClick = handler;
  }  

  @action.bound
  public setOnScroll(handler: (event: any) => void): void {
    this.onScroll = handler;
  }

  @action.bound
  public setOnKeyDown(handler: ((event: any) => void) | undefined): void {
    this.onKeyDown = handler;
  }
}

