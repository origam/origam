export interface IDispatcher {
  dispatch(event: any): void;
  downstreamDispatch(event: any): void;
  listen(cb: (event: any) => void): () => void;
  getRoot(): IDispatcher;
}