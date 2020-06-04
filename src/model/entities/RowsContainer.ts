import {action, computed, observable} from "mobx";
import {
  MAX_CHUNKS_TO_HOLD,
  SCROLL_ROW_CHUNK
} from "../../gui/Workbench/ScreenArea/TableView/InfiniteScrollLoader";
import {IFilterConfiguration} from "./types/IFilterConfiguration";
import {IOrderingConfiguration} from "./types/IOrderingConfiguration";

export interface IRowsContainer {
  clear(): void;

  delete(row: any[]): void;

  insert(index: number, row: any[]): void;

  set(rows: any[][]): void;

  substitute(row: any[]): void;

  registerResetListener(listener: () => void): void;

  maxRowCountSeen: number;

  rows: any[];
}

export class ListRowContainer implements IRowsContainer {
  private orderingConfiguration: IOrderingConfiguration;
  private filterConfiguration: IFilterConfiguration;

  constructor(orderingConfiguration: IOrderingConfiguration, filterConfiguration: IFilterConfiguration) {
    this.orderingConfiguration = orderingConfiguration;
    this.filterConfiguration = filterConfiguration;
  }

  @observable.shallow allRows: any[][] = [];
  rowIdGetter: (row: any[]) => string = null as any

  @computed get rows() {
    let rows = this.allRows;
    if (this.filterConfiguration.filteringFunction) {
      rows = rows.filter(row => this.filterConfiguration.filteringFunction()(row));
    }
    if(this.orderingConfiguration.ordering.length === 0){
      return rows;
    }else{
      return rows.sort(this.orderingConfiguration.orderingFunction());
    }
  }

  clear(): void {
    this.allRows.length = 0;
  }

  delete(row: any[]): void {
    const idx = this.allRows.findIndex(
      r => this.rowIdGetter(r) === this.rowIdGetter(row)
    );
    if (idx > -1) {
      this.allRows.splice(idx, 1);
    }
  }

  insert(index: number, row: any[]): void {
    const idx = this.allRows.findIndex(
      r => this.rowIdGetter(r) === this.rowIdGetter(row)
    );
    if (idx > -1) {
      this.allRows.splice(idx, 0, row);
    } else {
      this.allRows.push(row);
    }
  }

  set(rows: any[][]) {
    this.clear();
    this.allRows.push(...rows);
  }

  substitute(row: any[]): void {
    const idx = this.allRows.findIndex(
      r => this.rowIdGetter(r) === this.rowIdGetter(row)
    );
    if (idx > -1) {
      this.allRows.splice(idx, 1, row);
    }
  }

  get maxRowCountSeen() {
    return this.allRows.length;
  }

  registerResetListener(listener: () => void): void {
  }
}


export class ScrollRowContainer implements IRowsContainer {
  constructor(rowIdGetter: (row: any[]) => string) {
    this.rowIdGetter=rowIdGetter;
  }

  @observable
  rowChunks: RowChunk[] = [];
  private readonly rowIdGetter: (row: any[]) => string;
  _maxRowNumberSeen = 0;

  @computed
  get maxRowCountSeen() {
    const maxRowsNow = this.rowChunks.length === 0
      ? 0
      : this.rowChunks[this.rowChunks.length - 1].rowOffset + this.rowChunks[this.rowChunks.length - 1].length;
    if (maxRowsNow > this._maxRowNumberSeen) {
      this._maxRowNumberSeen = maxRowsNow;
    }

    return this._maxRowNumberSeen;
  }

  @computed
  get rows() {
    return this.rowChunks.flatMap(chunk => chunk.rows);
  }

  clear(): void {
    this.rowChunks.length = 0;
  }

  delete(row: any[]): void {
    throw new Error("Method not implemented");
  }

  insert(index: number, row: any[]): void {
    throw new Error("Method not implemented");
  }

  @action.bound
  set(rows: any[][]) {
    this.clear();
    this.rowChunks.push(new RowChunk(0, rows, this.rowIdGetter, undefined));
    this.notifyResetListeners()
  }

  substitute(row: any[]): void {
    for (let chunk of this.rowChunks) {
      const foundAndSubstituted = chunk.trySubstitute(row);
      if (foundAndSubstituted) {
        return;
      }
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
      : this.rowChunks[this.rowChunks.length - 1].rowOffset + SCROLL_ROW_CHUNK
  }

  get nextStartOffset() {
    return this.rowChunks.length === 0 || this.rowChunks[0].rowOffset < SCROLL_ROW_CHUNK
      ? 0
      : this.rowChunks[0].rowOffset - SCROLL_ROW_CHUNK;
  }

  @action.bound
  prependRecords(rows: any[][]) {
    if (this.rowChunks.length === 0) {
      this.rowChunks.push(new RowChunk(0, rows, this.rowIdGetter, undefined));
      return;
    }
    const rowOffset = this.rowChunks[0].rowOffset - SCROLL_ROW_CHUNK;
    this.rowChunks.unshift(new RowChunk(rowOffset, rows, this.rowIdGetter, undefined))
    if (this.rowChunks.length > MAX_CHUNKS_TO_HOLD) {
      this.rowChunks.pop();
    }
  }

  @action.bound
  appendRecords(rows: any[][]) {
    if (this.rowChunks.length === 0) {
      this.rowChunks.push(new RowChunk(0, rows, this.rowIdGetter, undefined));
      return;
    }
    const rowOffset = this.rowChunks[this.rowChunks.length - 1].rowOffset + SCROLL_ROW_CHUNK;
    const filteredRows = this.filterDuplicateRecords(rows);
    const isFinal = rows.length < SCROLL_ROW_CHUNK;
    this.rowChunks.push(new RowChunk(rowOffset, filteredRows, this.rowIdGetter, isFinal))
    if (this.rowChunks.length > MAX_CHUNKS_TO_HOLD) {
      this.rowChunks.shift();
    }
  }

  filterDuplicateRecords(rows: any[][]){
    return rows;
    // return rows.filter(row => !this.isAlreadyInAChunk(row))
  }

  isAlreadyInAChunk(row: any[]){
    return this.rowChunks
      .some(rowChunk => rowChunk.has(row));
  }

  @computed
  get isLastRowLoaded() {
    return this.rowChunks.length === 0
      ? false
      : this.rowChunks[this.rowChunks.length - 1].isFinal;
  }

  @computed
  get isFirstRowLoaded() {
    return this.rowChunks.length === 0
      ? false
      : this.rowChunks[0].isInitial;
  }

  @computed
  get isFull(){
    return this.rows.length === MAX_CHUNKS_TO_HOLD * SCROLL_ROW_CHUNK;
  }

}


class RowChunk {
  rowOffset: number;
  rows: any[];
  private rowIdGetter: (row: any[]) => string;
  isFinal: boolean;

  constructor(rowOffset: number, rows: any[], rowIdGetter: (row: any[]) => string, isFinal: boolean | undefined) {
    this.rowIdGetter = rowIdGetter;
    this.isFinal = isFinal === undefined
      ? rows.length < SCROLL_ROW_CHUNK
      :isFinal  ;
    if (rowOffset < 0) {
      throw new Error("Offset cannot be less than 0");
    }
    this.rowOffset = rowOffset;
    this.rows = rows;
  }

  get isInitial() {
    return this.rowOffset === 0;
  }

  get length() {
    return this.rows.length;
  }

  trySubstitute(row: any[]) {
    const index = this.rows.findIndex(row => this.rowIdGetter(row) === this.rowIdGetter(row)
    );
    if (index > -1) {
      this.rows.splice(index, 1, row);
      return true;
    } else {
      return false;
    }
  }

  has(row: any[]) {
    return this.rows.some(chunkRow => this.rowIdGetter(chunkRow) === this.rowIdGetter(row));
  }
}

