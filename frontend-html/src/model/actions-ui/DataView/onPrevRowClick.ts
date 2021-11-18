import {selectPrevRow} from "model/actions/DataView/selectPrevRow";
import {flow} from "mobx";
import {handleError} from "model/actions/handleError";
import { getDataView } from "model/selectors/DataView/getDataView";
import { shouldProceedToChangeRow } from "./TableView/shouldProceedToChangeRow";

export function onPrevRowClick(ctx: any) {
  return flow(function* onPrevRowClick(event: any) {
    try {
      const dataView = getDataView(ctx);
      if (!(yield shouldProceedToChangeRow(dataView))) {
        return;
      }
      yield* selectPrevRow(ctx)();
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
