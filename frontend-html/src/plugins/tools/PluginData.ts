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

import { IPluginData } from "../types/IPluginData";
import { IDataView } from "../../model/entities/types/IDataView";
import { IPluginTableRow } from "../types/IPluginRow";
import { getProperties } from "../../model/selectors/DataView/getProperties";
import { IPluginDataView } from "../types/IPluginDataView";
import { IPluginProperty } from "../types/IPluginProperty";


export function createPluginData(dataView: IDataView): IPluginData | undefined {
  if (!dataView) {
    return undefined;
  }
  return {
    dataView: new PluginDataView(dataView)
  }
}

class PluginDataView implements IPluginDataView {
  properties: IPluginProperty[];

  get tableRows(): IPluginTableRow[] {
    return this.dataView.tableRows;
  }

  constructor(
    private dataView: IDataView,
  ) {
    this.properties = getProperties(this.dataView);
  }

  getCellText(row: any[], propertyId: string): any {
    const property = getProperties(this.dataView).find(prop => prop.id === propertyId);
    if (!property) {
      throw new Error("Property named \"" + propertyId + "\" was not found");
    }
    return this.dataView.dataTable.getCellText(row, property);
  }

  getRowId(row: IPluginTableRow) {
    return Array.isArray(row)
      ? this.dataView.dataTable.getRowId(row)
      : row.columnLabel + row.columnValue + row.groupLevel + row.isExpanded;
  }
}

