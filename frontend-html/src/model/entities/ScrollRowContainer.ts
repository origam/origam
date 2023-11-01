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

import { action, computed, observable } from "mobx";
import { MAX_CHUNKS_TO_HOLD, SCROLL_ROW_CHUNK, } from "../../gui/Workbench/ScreenArea/TableView/InfiniteScrollLoader";
import { IRowsContainer } from "./types/IRowsContainer";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { fixRowIdentifier } from "utils/dataRow";
import { IProperty } from "model/entities/types/IProperty";

// The constants have to be defined here for the unit tests to work.
// const MAX_CHUNKS_TO_HOLD = 20;
// const SCROLL_ROW_CHUNK = 1000;

export class ScrollRowContainer implements IRowsContainer {
  $type_ScrollRowContainer: 1 = 1;

  constructor(rowIdGetter: (row: any[]) => string, parent: any) {
    this.rowIdGetter = rowIdGetter;
    this.parent = parent;
  }

  parent: any;
  @observable
  rowChunks: RowChunk[] = [];
  private readonly rowIdGetter: (row: any[]) => string;

  getTrueIndexById(id: string) {
    const chunk = this.findChunkByRowId(id);
    if (!chunk) {
      return undefined;
    }
    return chunk.rowOffset + chunk.getRowIdxById(id)!;
  }

  async updateSortAndFilter(data?: { retainPreviousSelection?: true }) {
  }

  getRowById(id: string): any[] | undefined {
    return this.rows.find((row) => this.rowIdGetter
    (row) === id);
  }

  start() {
  }

  stop() {
  }

  @computed
  get rows() {
    return this.rowChunks.flatMap((chunk) => chunk.rows);
  }

  get allRows() {
    return this.rows;
  }

  getAllValuesOfProp(property: IProperty) {
    return this.rows;
  }

  clear(): void {
    this.rowChunks.length = 0;
  }

  delete(row: any[]): void {
    const rowId = this.rowIdGetter(row);
    const chunk = this.findChunkByRowId(rowId);
    if (!chunk) {
      throw new Error(`Row with id "${rowId}" was not found`);
    }
    chunk.delete(rowId);
  }

  findChunkByRowId(rowId: string) {
    return this.rowChunks.find((chunk) => chunk.has(rowId));
  }

  findChunkByRowIndex(indexInContainer: number) {
    let rowCounter = 0;
    if (this.rowChunks.length === 0) {
      return {
        chunk: undefined,
        indexInChunk: undefined,
      };
    }
    for (let rowChunk of this.rowChunks) {
      const indexInChunk = indexInContainer - rowCounter;
      if (indexInChunk < rowChunk.rows.length) {
        return {
          chunk: rowChunk,
          indexInChunk: indexInChunk,
        };
      }
      rowCounter += rowChunk.rows.length;
    }
    const lastChunk = this.rowChunks[this.rowChunks.length - 1];
    return {
      chunk: lastChunk,
      indexInChunk: lastChunk.rows.length,
    };
  }

  insert(index: number, row: any[], shouldLockNewRowAtTop?: boolean): Promise<any> {
    const dataTable = getDataTable(this);
    row = fixRowIdentifier(row, dataTable.identifierDataIndex);
    const {chunk, indexInChunk} = this.findChunkByRowIndex(index);
    if (!chunk) {
      this.rowChunks.push(new RowChunk(0, [row], this.rowIdGetter, undefined));
    } else {
      chunk.insert(indexInChunk!, row);
    }
    return Promise.resolve();
  }

  @action.bound
  async set(rowsIn: any[][], rowOffset: number = 0, isFinal: boolean | undefined = undefined): Promise<any> {
    const dataTable = getDataTable(this);
    const rows: any[][] = [];
    for (let row of rowsIn) {
      rows.push(fixRowIdentifier(row, dataTable.identifierDataIndex));
    }
    this.clear();
    if (rows.length !== 0) {
      this.rowChunks.push(new RowChunk(rowOffset, rows, this.rowIdGetter, isFinal));
    }
    this.notifyResetListeners();
  }

  substitute(row: any[]): void {
    const dataTable = getDataTable(this);
    row = fixRowIdentifier(row, dataTable.identifierDataIndex);
    for (let chunk of this.rowChunks) {
      const foundAndSubstituted = chunk.trySubstitute(row);
      if (foundAndSubstituted) {
        return;
      }
    }
  }

  substituteRows(rows: any[][]){
    for (let row of rows) {
      this.substitute(row);
    }
  }

  resetListeners: (() => void)[] = [];

  registerResetListener(listener: () => void) {
    this.resetListeners.push(listener);
  }

  notifyResetListeners() {
    for (let resetListener of this.resetListeners) {
      resetListener();
    }
  }

  get nextEndOffset() {
    return this.rowChunks.length === 0
      ? SCROLL_ROW_CHUNK
      : this.rowChunks[this.rowChunks.length - 1].rowOffset + SCROLL_ROW_CHUNK;
  }

  get nextStartOffset() {
    return this.rowChunks.length === 0 || this.rowChunks[0].rowOffset < SCROLL_ROW_CHUNK
      ? 0
      : this.rowChunks[0].rowOffset - SCROLL_ROW_CHUNK;
  }

  @action.bound
  prependRecords(rowsIn: any[][]) {
    const dataTable = getDataTable(this);
    const rows: any[][] = [];
    for (let row of rowsIn) {
      rows.push(fixRowIdentifier(row, dataTable.identifierDataIndex));
    }
    if (this.rowChunks.length === 0) {
      this.rowChunks.push(new RowChunk(0, rows, this.rowIdGetter, undefined));
      return;
    }
    const rowOffset = this.rowChunks[0].rowOffset - SCROLL_ROW_CHUNK;
    if (rowOffset < 0) {
      return;
    }
    this.rowChunks.unshift(new RowChunk(rowOffset, rows, this.rowIdGetter, undefined));
    if (this.rowChunks.length > MAX_CHUNKS_TO_HOLD) {
      this.rowChunks.pop();
    }
  }

  @action.bound
  appendRecords(rowsIn: any[][]) {
    const dataTable = getDataTable(this);
    const rows: any[][] = [];
    for (let row of rowsIn) {
      rows.push(fixRowIdentifier(row, dataTable.identifierDataIndex));
    }
    if (this.rowChunks.length === 0) {
      this.rowChunks.push(new RowChunk(0, rows, this.rowIdGetter, undefined));
      return;
    }
    const rowOffset = this.rowChunks[this.rowChunks.length - 1].rowOffset + SCROLL_ROW_CHUNK;
    const isFinal = rows.length < SCROLL_ROW_CHUNK;
    const rowChunk = new RowChunk(rowOffset, rows, this.rowIdGetter, isFinal);
    const filteredChunk = this.replaceDuplicateRows(rowChunk);
    this.rowChunks.push(filteredChunk);
    if (this.rowChunks.length > MAX_CHUNKS_TO_HOLD) {
      this.rowChunks.shift();
    }
  }

  replaceDuplicateRows(newChunk: RowChunk) {
    let filteredChunk = newChunk;
    for (let rowChunk of this.rowChunks) {
      filteredChunk = rowChunk.replaceRows(filteredChunk);
    }
    return filteredChunk;
  }

  @computed
  get isLastRowLoaded() {
    return this.rowChunks.length === 0 ? false : this.rowChunks[this.rowChunks.length - 1].isFinal;
  }

  @computed
  get isFirstRowLoaded() {
    return this.rowChunks.length === 0 ? false : this.rowChunks[0].isInitial;
  }

  @computed
  get isFull() {
    return this.rows.length === MAX_CHUNKS_TO_HOLD * SCROLL_ROW_CHUNK;
  }

  unlockAddedRowPosition(): void {
  }

  addedRowPositionLocked: boolean = false;

  getFirstRow(): any[] | undefined {
    if (this.rows.length === 0) {
      return undefined;
    }
    return this.rows[0];
  }

  getFilteredRows(args: { propertyFilterIdToExclude: string }): any[] {
    return [];
  }
}

class RowChunk {
  rowOffset: number;
  @observable.shallow
  rows: any[];
  private rowIdGetter: (row: any[]) => string;
  isFinal: boolean;
  private idMap: Map<string, number>;

  constructor(
    rowOffset: number,
    rows: any[],
    rowIdGetter: (row: any[]) => string,
    isFinal: boolean | undefined
  ) {
    this.rowIdGetter = rowIdGetter;
    this.isFinal = isFinal === undefined ? rows.length < SCROLL_ROW_CHUNK : isFinal;
    if (rowOffset < 0) {
      throw new Error("Offset cannot be less than 0");
    }
    this.rowOffset = rowOffset;
    this.rows = rows;
    this.idMap = this.makeIdMap(rows);
  }

  makeIdMap(rows: any[][]) {
    const idMap = new Map<string, number>();
    for (let i = 0; i < rows.length; i++) {
      idMap.set(this.rowIdGetter(rows[i]), i);
    }
    return idMap;
  }

  get isInitial() {
    return this.rowOffset === 0;
  }

  get length() {
    return this.rows.length;
  }

  trySubstitute(row: any[]) {
    const index = this.rows.findIndex(
      (existingRow) => this.rowIdGetter(existingRow) === this.rowIdGetter(row)
    );
    if (index > -1) {
      this.rows.splice(index, 1, row);
      return true;
    } else {
      return false;
    }
  }

  getRow(id: string) {
    const rowIndex = this.idMap.get(id);
    if (rowIndex === undefined) {
      return undefined;
    }
    return this.rows[rowIndex];
  }

  replaceRows(sourceChunk: RowChunk) {
    const duplicateRowIndicesInSource: number[] = [];
    for (let id of this.idMap.keys()) {
      const duplicateRow = sourceChunk.getRow(id);
      if (duplicateRow) {
        const indexInThisChunk = this.idMap.get(id)!;
        this.rows[indexInThisChunk] = duplicateRow;
        duplicateRowIndicesInSource.push(sourceChunk.getIndex(id)!);
      }
    }

    if (duplicateRowIndicesInSource.length === 0) {
      return sourceChunk;
    } else {
      const nonDuplicateRows = sourceChunk.rows.filter(
        (row, i) => !duplicateRowIndicesInSource.includes(i)
      );
      return new RowChunk(
        sourceChunk.rowOffset,
        nonDuplicateRows,
        this.rowIdGetter,
        sourceChunk.isFinal
      );
    }
  }

  private getIndex(rowId: string) {
    return this.idMap.get(rowId);
  }

  has(rowId: string) {
    return this.getIndex(rowId) !== undefined;
  }

  delete(rowId: string) {
    const index = this.getIndex(rowId)!;
    this.idMap.delete(rowId);
    this.rows.splice(index, 1);
    this.shiftIdMapDown(index);
  }

  insert(index: number, row: any[]) {
    this.rows.splice(index, 0, row);
    const rowId = this.rowIdGetter(row);
    this.shiftIdMapUp(index);
    this.idMap.set(rowId, index);
  }

  private shiftIdMapDown(index: number) {
    for (let entry of Array.from(this.idMap.entries())) {
      if (entry[1] > index) {
        this.idMap.set(entry[0], entry[1] - 1);
      }
    }
  }

  private shiftIdMapUp(index: number) {
    for (let entry of Array.from(this.idMap.entries())) {
      if (entry[1] > index - 1) {
        this.idMap.set(entry[0], entry[1] + 1);
      }
    }
  }

  getRowIdxById(id: string) {
    const idx = this.rows.findIndex((row) => this.rowIdGetter(row) === id);
    return idx > -1 ? idx : undefined;
  }
}

export const isScrollRowContainer = (o: any): o is ScrollRowContainer => o.$type_ScrollRowContainer;
