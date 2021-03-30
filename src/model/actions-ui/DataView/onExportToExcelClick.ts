import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import {getDataView} from "model/selectors/DataView/getDataView";

export function onExportToExcelClick(ctx: any) {
  return flow(function* onExportToExcelClick(event: any) {
    try {
      getDataView(ctx).exportToExcel();
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}