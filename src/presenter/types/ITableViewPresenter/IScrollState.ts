export interface IScrollState {
  scrollTop: number;
  scrollLeft: number;
  setScrollOffset(event: any, scrollTop: number, scrollLeft: number): void;
}