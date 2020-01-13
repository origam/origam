import { getDataView } from "model/selectors/DataView/getDataView"

export default {
  toggleSelectedId(ctx: any) {
    return function* toggleSelectedId(id: string) {
      getDataView(ctx).toggleSelectedRowId(id);
    }
  } 
}