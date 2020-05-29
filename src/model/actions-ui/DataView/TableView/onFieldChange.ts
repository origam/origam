import { getFormScreenLifecycle } from "../../../selectors/FormScreen/getFormScreenLifecycle";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { getDataView } from "model/selectors/DataView/getDataView";
import { IProperty } from "model/entities/types/IProperty";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { flushCurrentRowData } from "model/actions/DataView/TableView/flushCurrentRowData";

export function onFieldChange(ctx: any) {
  return flow(onFieldChangeG(ctx));
}

export function onFieldChangeG(ctx: any) {
  return function* onFieldChange(event: any, row: any[], property: IProperty, value: any) {
    try {
      if (property.column === "ComboBox") {
        value = `${value}`;
      }
      getDataView(ctx).onFieldChange(event, row, property, value);
      if (
        property.column === "ComboBox" ||
        property.column === "CheckBox" ||
        property.column === "Checklist" ||
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

export function changeManyFields(ctx: any) {
  return function* changeManyFields(values: Array<{ fieldId: string; value: any }>) {
    const dataTable = getDataTable(ctx);
    const row = getSelectedRow(ctx);
    if (row) {
      for (let { fieldId, value } of values) {
        dataTable.setDirtyValue(row, fieldId, value);
      }
    }
  };
}
