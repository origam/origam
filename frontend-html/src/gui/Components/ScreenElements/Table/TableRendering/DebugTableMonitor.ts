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

interface TableDebugData {
  data: Array<Record<string, any>>;
  rendered: boolean;
  renderRevision: number;
  renderedRevision: number;
}

const renderTimers = new Map<string, ReturnType<typeof setTimeout>>();

export function clearTableDebugValues(ctx: any){
  const dataView = getDataView(ctx);
  const tableId = dataView.modelInstanceId;
  const win = (window as any);
  const tableData = ensureTableDataExists(win, tableId);
  const renderTimer = renderTimers.get(tableId);
  if (renderTimer) {
    clearTimeout(renderTimer);
    renderTimers.delete(tableId);
  }
  tableData.data = [];
  tableData.rendered = false;
  tableData.renderRevision++;
}

function ensureTableDataExists(win: any, tableId: string): TableDebugData {
  if (!win.tableDebugMonitor) {
    win.tableDebugMonitor = {};
  }
  if (!win.tableDebugMonitor[tableId]) {
    win.tableDebugMonitor[tableId] = {
      data: [],
      rendered: false,
      renderRevision: 0,
      renderedRevision: 0,
    }
  }
  return win.tableDebugMonitor[tableId];
}

export function setTableDebugRendered(ctx: any){
  const dataView = getDataView(ctx);
  const tableId = dataView.modelInstanceId;
  const win = (window as any);
  const renderRevision = ensureTableDataExists(win, tableId).renderRevision;
  const currentTimer = renderTimers.get(tableId);
  if (currentTimer) {
    clearTimeout(currentTimer);
  }
  renderTimers.set(tableId, setTimeout(() => {
    renderTimers.delete(tableId);
    const tableData = ensureTableDataExists(win, tableId);
    if (tableData.renderRevision === renderRevision) {
      tableData.rendered = true;
      tableData.renderedRevision = renderRevision;
    }
  }, 200));
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

