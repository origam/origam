import { getRowStates } from "model/selectors/RowState/getRowStates";

export function putRowStateValue(ctx: any) {
  return function putRowStateValue(state: any) {
    getRowStates(ctx).putValue(state);
  };
}
