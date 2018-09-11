export interface IGridProps {
  view: IGridView;
  width: number;
  height: number;
  overlayElements: React.Component | React.Component[] | null;
}

export interface IGridView {
  componentDidMount(
    props: IGridProps,
    component: React.Component<IGridProps>
  ): void;

  componentDidUpdate(
    prevProps: IGridProps,
    props: IGridProps,
    component: React.Component<IGridProps>
  ): void;

  componentWillUnmount(): void;

  canvasProps: {
    width: number;
    height: number;
    style: {
      width: number;
      height: number;
    };
  };

  contentWidth: number;
  contentHeight: number;

  handleGridScroll(event: any): void;
  handleGridKeyDown(event: any): void;
  handleGridClick(event: any): void;
  refRoot(element: HTMLDivElement): void;
  refScroller(element: HTMLDivElement): void;
  refCanvas(element: HTMLCanvasElement): void;
}

export interface IGridSelectors {
  width: number;
  height: number;
  innerWidth: number;
  innerHeight: number;
}

export interface IGridState {
  width: number;
  height: number;

  setSize(width: number, height: number): void;
  setRefRoot(element: HTMLDivElement): void;
  setRefScroller(element: HTMLDivElement): void;
  setRefCanvas(element: HTMLCanvasElement): void;
}

export interface IGridActions {
  handleResize(width: number, height: number): void;
  refRoot(element: HTMLDivElement): void;
  refScroller(element: HTMLDivElement): void;
  refCanvas(element: HTMLCanvasElement): void;
}