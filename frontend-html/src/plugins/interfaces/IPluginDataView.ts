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

import { IPluginTableRow } from "./IPluginTableRow";
import { IPluginProperty } from "./IPluginProperty";

export interface IPluginDataView {
  tableRows: IPluginTableRow[];
  properties: IPluginProperty[];
  entity: string;

  getCellText(row: any, propertyId: string): any;

  getCellValue(row: any, propertyId: string): any;

  getRowId(row: IPluginTableRow): string;

  saveConfiguration(pluginName: string, configuration: string): Promise<void>;

  getConfiguration(pluginName: string): string | undefined;
}