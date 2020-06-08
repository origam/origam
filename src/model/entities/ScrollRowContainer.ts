import {action, computed, observable} from "mobx";
import {MAX_CHUNKS_TO_HOLD, SCROLL_ROW_CHUNK} from "../../gui/Workbench/ScreenArea/TableView/InfiniteScrollLoader";
import {IRowsContainer} from "./types/IRowsContainer";


export class ScrollRowContainer implements IRowsContainer {
  constructor(rowIdGetter: (row: any[]) => string) {
    this.rowIdGetter = rowIdGetter;
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
    const isFinal = rows.length < SCROLL_ROW_CHUNK;
    const rowChunk = new RowChunk(rowOffset, rows, this.rowIdGetter, isFinal);
    const filteredChunk = this.replaceDuplicateRows(rowChunk);
    this.rowChunks.push(filteredChunk)
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
  get isFull() {
    return this.rows.length === MAX_CHUNKS_TO_HOLD * SCROLL_ROW_CHUNK;
  }

}


class RowChunk {
  rowOffset: number;
  rows: any[];
  private rowIdGetter: (row: any[]) => string;
  isFinal: boolean;
  private idMap: Map<string, number> ;

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
    this.idMap = this.makeIdMap(rows);
  }

  makeIdMap(rows: any[][]){
    const idMap = new Map<string, number>();
    for (let i = 0; i<rows.length; i++) {
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
    const index = this.rows.findIndex(row => this.rowIdGetter(row) === this.rowIdGetter(row)
    );
    if (index > -1) {
      this.rows.splice(index, 1, row);
      return true;
    } else {
      return false;
    }
  }

  getRow(id: string){
    const rowIndex = this.idMap.get(id);
    if(rowIndex === undefined){
      return undefined
    }
    return this.rows[rowIndex];
  }

  replaceRows(sourceChunk: RowChunk) {
    const duplicateRowIndicesInSource: number[]=[];
    for (let id of this.idMap.keys()) {
      const duplicateRow = sourceChunk.getRow(id);
      if(duplicateRow){
        const indexInThisChunk = this.idMap.get(id)!;
        this.rows[indexInThisChunk] = duplicateRow;
        duplicateRowIndicesInSource.push(sourceChunk.getIndex(id)!)
      }
    }

    if(duplicateRowIndicesInSource.length === 0 ){
      return sourceChunk;
    }else{
      const nonDuplicateRows = sourceChunk.rows
        .filter((row,i) => !duplicateRowIndicesInSource.includes(i))
      return new RowChunk(sourceChunk.rowOffset, nonDuplicateRows, this.rowIdGetter, sourceChunk.isFinal);
    }
  }

  private getIndex(id: string) {
    return this.idMap.get(id);
  }
}

