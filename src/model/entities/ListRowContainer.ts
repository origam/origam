import {computed, observable, action, reaction, IReactionDisposer} from "mobx";
import { IFilterConfiguration } from "./types/IFilterConfiguration";
import { IOrderingConfiguration } from "./types/IOrderingConfiguration";
import { IRowsContainer } from "./types/IRowsContainer";
import { trace } from "mobx"

export class ListRowContainer implements IRowsContainer {
  private orderingConfiguration: IOrderingConfiguration;
  private filterConfiguration: IFilterConfiguration;
  private reactionDisposer: IReactionDisposer | undefined;
  @observable
  private forcedFirstRowId: string | undefined;
  constructor(
    orderingConfiguration: IOrderingConfiguration,
    filterConfiguration: IFilterConfiguration,
    rowIdGetter: (row: any[]) => string
  ) {
    this.orderingConfiguration = orderingConfiguration;
    this.filterConfiguration = filterConfiguration;
    this.rowIdGetter = rowIdGetter;
  }

  @observable.shallow allRows: any[][] = [];
  rowIdGetter: (row: any[]) => any;

  @observable
  sortedIds: any[] | undefined;

  @computed get idToRow() {
    const entries = this.allRows.map(row => [this.rowIdGetter(row), row] as [any, any[]]);
    return new Map<any, any[]>(entries);
  }

  start() {
    this.reactionDisposer = reaction(
      () => [this.filterConfiguration.filters, this.orderingConfiguration.orderings],
      () => this.updateSortAndFilter(),
      {fireImmediately: true}
    );
  }

  stop(){
    this.reactionDisposer?.();
  }

  @action
  updateSortAndFilter() {
    let rows = this.allRows;
    if (this.filterConfiguration.filteringFunction) {
      rows = rows.filter((row) => this.filterConfiguration.filteringFunction()(row));
    }
    if (this.orderingConfiguration.orderings.length !== 0) {
      rows = rows.sort((row1: any[], row2: any[]) => this.internalRowOrderingFunc(row1, row2));
    }
    this.sortedIds = rows.map(row => this.rowIdGetter(row));
  }

  @computed get rows() {
    if(!this.sortedIds){
      return this.allRows;
    }
    return this.sortedIds
      .map(id => this.idToRow.get(id))
      .filter(row => row) as any[][];
  }

  @computed get loadedRowsCount() {
    return this.rows.length;
  }

  internalRowOrderingFunc(row1: any[], row2: any[]) {
    if(this.forcedFirstRowId !== undefined){
      if (this.forcedFirstRowId === this.rowIdGetter(row1)) return -1;
      if (this.forcedFirstRowId === this.rowIdGetter(row2)) return 1;
    }
    return this.orderingConfiguration.orderingFunction()(row1, row2);
  }

  unlockAddedRowPosition(): void {
    this.forcedFirstRowId = undefined;
  }

  clear(): void {
    this.allRows.length = 0;
  }

  delete(row: any[]): void {
    const idx = this.allRows.findIndex((r) => this.rowIdGetter(r) === this.rowIdGetter(row));
    if (idx > -1) {
      this.allRows.splice(idx, 1);
    }
  }

  insert(index: number, row: any[]): void {
    this.allRows.splice(index, 0, row);
    this.forcedFirstRowId = this.rowIdGetter(row);
  }

  set(rows: any[][]) {
    this.clear();
    this.allRows.push(...rows);
    this.updateSortAndFilter();
  }

  substitute(row: any[]): void {
    const idx = this.allRows.findIndex((r) => this.rowIdGetter(r) === this.rowIdGetter(row));
    if (idx > -1) {
      this.allRows.splice(idx, 1, row);
    }
  }

  get maxRowCountSeen() {
    return this.rows.length;
  }

  registerResetListener(listener: () => void): void {}
}
