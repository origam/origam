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

export interface IRowsContainer {
  clear(): void;

  delete(row: any[]): void;

  insert(index: number, row: any[], shouldLockNewRowAtTop?: boolean): Promise<any>;

  set(rows: any[][], rowOffset: number, isFinal: boolean | undefined): Promise<any>;

  appendRecords(rowsIn: any[][]): void;

  substituteRows(rows: any[][]): void;

  registerResetListener(listener: () => void): void;

  unlockAddedRowPosition(): void;

  addedRowPositionLocked: boolean;

  rows: any[];

  allRows: any[];

  getFilteredRows(args: { propertyFilterIdToExclude: string }): any[];

  updateSortAndFilter(data?: { retainPreviousSelection?: true }): Promise<any>;

  getRowById(id: string): any[] | undefined;

  start(): void;

  stop(): void;

  getFirstRow(): any[] | undefined;

  parent?: any;

  getTrueIndexById(id: string): number | undefined;
}
