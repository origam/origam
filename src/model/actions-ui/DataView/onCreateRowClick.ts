import { getGridId } from "model/selectors/DataView/getGridId";
import { getEntity } from "model/selectors/DataView/getEntity";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { getDataView } from "model/selectors/DataView/getDataView";
import { shouldProceedToChangeRow } from "./TableView/shouldProceedToChangeRow";

export function onCreateRowClick(ctx: any) {
  return flow(function* onCreateRowClick(event: any) {
    try {
      const gridId = getGridId(ctx);
      const entity = getEntity(ctx);
      const formScreenLifecycle = getFormScreenLifecycle(ctx);
      const dataView = getDataView(ctx);
      if (!(yield shouldProceedToChangeRow(dataView))) {
        return;
      }
      yield* formScreenLifecycle.onCreateRow(entity, gridId);
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}

