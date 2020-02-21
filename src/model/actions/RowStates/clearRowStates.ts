import { getRowStates } from "model/selectors/RowState/getRowStates";
import { getDataSources } from "model/selectors/DataSources/getDataSources";

export function clearRowStates(ctx: any) {
  return function* clearRowStates() {
    for (let dataSource of getDataSources(ctx)) {
      getRowStates(dataSource).clearAll();
    }
  };
}
