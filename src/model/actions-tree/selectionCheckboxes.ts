import {getDataView} from "model/selectors/DataView/getDataView"

export function hasSelectedRowId(ctx: any, id: string) {
   return getDataView(ctx).hasSelectedRowId(id);
}

export function setSelectedStateRowId(ctx: any) {
  return function* toggleSelectedId(id: string, newState: boolean) {
    getDataView(ctx).setSelectedState(id, newState);
  }
}
