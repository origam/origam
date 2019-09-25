import { getFormScreenLifecycle } from "../../../selectors/FormScreen/getFormScreenLifecycle";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { getDataView } from "model/selectors/DataView/getDataView";
import { IProperty } from "model/entities/types/IProperty";
export function onFieldChange(ctx: any) {
  return function onFieldChange(event: any, row: any[], property: IProperty, value: any) {
    console.log("ofc");
    getDataView(ctx).onFieldChange(event, row, property, value);
    if (property.column === "ComboBox") {
      // Flush data to session when combo value changed.
      getDataTable(ctx).flushFormToTable(row);
      getFormScreenLifecycle(ctx).onFlushData();
    }
  };
}
