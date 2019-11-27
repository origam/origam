import { getFormScreenLifecycle } from "../../../selectors/FormScreen/getFormScreenLifecycle";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { getDataView } from "model/selectors/DataView/getDataView";
import { IProperty } from "model/entities/types/IProperty";
import { flow } from "mobx";

export function onFieldChange(ctx: any) {
  return flow(function* onFieldChange(
    event: any,
    row: any[],
    property: IProperty,
    value: any
  ) {
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
  });
}
