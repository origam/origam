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

import {getDataView} from "model/selectors/DataView/getDataView";
import _ from "lodash";

export function clearTableDebugValues(ctx: any){
  const dataView = getDataView(ctx);
  const tableId = dataView.modelInstanceId;
  const win = (window as any);
  if(win.tableDebugMonitor && win.tableDebugMonitor[tableId]){
    win.tableDebugMonitor[tableId].data = [];
  }
}
export const setTableDebugRendered = _.debounce(setRenderedInternal, 200);

function ensureTableDataExists(win: any, tableId: string) {
  if (!win.tableDebugMonitor) {
    win.tableDebugMonitor = {};
  }
  if (!win.tableDebugMonitor[tableId]) {
    win.tableDebugMonitor[tableId] = {
      data: [],
      rendered: false
    }
  }
}

export function setRenderedInternal(ctx: any){
  const dataView = getDataView(ctx);
  const tableId = dataView.modelInstanceId;
  const win = (window as any);
  ensureTableDataExists(win, tableId);
  win.tableDebugMonitor[tableId].rendered = true;
}

export function setTableDebugValue(ctx: any, propertyId: string | null, rowIndex: number, value: any){
  if(propertyId == null){
    return;
  }
  const dataView = getDataView(ctx);
  const tableId = dataView.modelInstanceId;
  const win = (window as any);
  ensureTableDataExists(win, tableId);
  if(!win.tableDebugMonitor[tableId].data[rowIndex]){
    win.tableDebugMonitor[tableId].data[rowIndex] = {};
  }
  win.tableDebugMonitor[tableId].data[rowIndex][propertyId] = value;
}

