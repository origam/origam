/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

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
  return function*onFieldChange(args: { event: any, row: any[], property: IProperty, value: any }) {
    const {property, row, event} = args;
    let value = args.value;
    try {
      if (property.column === "ComboBox" && value !== null) {
        value = `${value}`;
      }
      getDataView(ctx).onFieldChange(event, row, property, value);
      if (
        property.column === "TagInput" ||
        property.column === "ComboBox" ||
        property.column === "CheckBox" ||
        property.column === "Checklist" ||
        (property.column === "Date" && event.type === "click")
      ) {
        // Flush data to session when combo value changed.
        getDataTable(ctx).flushFormToTable(row);
        yield*getFormScreenLifecycle(ctx).onFlushData();
      }
    } catch (e) {
      yield*handleError(ctx)(e);
      throw e;
    }
  };
}

export function changeManyFields(ctx: any) {
  return function*changeManyFields(values: Array<{ fieldId: string; value: any }>) {
    const dataTable = getDataTable(ctx);
    const row = getSelectedRow(ctx);
    if (row) {
      for (let {fieldId, value} of values) {
        dataTable.setDirtyValue(row, fieldId, value);
      }
    }
  };
}
