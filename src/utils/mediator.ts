export interface IDispatcher {
  dispatch(event: any): void;
  downstreamDispatch(event: any): void;
  listen(cb: (event: any) => void): () => void;
  getParent(): IDispatcher;
}

export const NS = "GLOBAL";

export const STATE_VARIABLE_CHANGED = `${NS}/STATE_VARIABLE_CHANGED`;

export const stateVariableChanged = () => ({ type: STATE_VARIABLE_CHANGED });
