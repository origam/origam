import {computed, observable} from "mobx";
import {IFilterConfiguration} from "./types/IFilterConfiguration";
import {IOrderingConfiguration} from "./types/IOrderingConfiguration";
import {IRowsContainer} from "./types/IRowsContainer";

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

