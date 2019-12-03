import { getFormScreenLifecycle } from "../../../selectors/FormScreen/getFormScreenLifecycle";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { getDataView } from "model/selectors/DataView/getDataView";
import { IProperty } from "model/entities/types/IProperty";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";

export function onFieldChange(ctx: any) {
  return flow(onFieldChangeG(ctx));
}

export function onFieldChangeG(ctx: any) {
  return function* onFieldChange(
    event: any,
    row: any[],
    property: IProperty,
    value: any
  ) {
    try {
      getDataView(ctx).onFieldChange(event, row, property, value);
      if (
        property.column === "ComboBox" ||
        property.column === "CheckBox" ||
        (property.column === "Date" && event.type === "click")
      ) {
        // Flush data to session when combo value changed.
        getDataTable(ctx).flushFormToTable(row);
        yield* getFormScreenLifecycle(ctx).onFlushData();
      }
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  };
}
