import {selectNextRow} from "model/actions/DataView/selectNextRow";
import {flow} from "mobx";
import {handleError} from "model/actions/handleError";
import { getDataView } from "model/selectors/DataView/getDataView";
import { shouldProceedToChangeRow } from "./TableView/shouldProceedToChangeRow";

export function onNextRowClick(ctx: any) {
  return flow(function* onNextRowClick(event: any) {
    try {
      const dataView = getDataView(ctx);
      if (!(yield shouldProceedToChangeRow(dataView))) {
        return;
      }
      yield* selectNextRow(ctx)();
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
