import {getRowStatesByEntity} from "model/selectors/RowState/getRowStatesByEntity";

export function putRowStateValue(ctx: any) {
  return function putRowStateValue(entity: string, state: any) {
    const rowStates = getRowStatesByEntity(ctx, entity);
    rowStates && rowStates.putValue(state);
  };
}
