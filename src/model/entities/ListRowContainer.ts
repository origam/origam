import {computed, observable, action, reaction, IReactionDisposer, flow} from "mobx";
import { IFilterConfiguration } from "./types/IFilterConfiguration";
import { IOrderingConfiguration } from "./types/IOrderingConfiguration";
import { IRowsContainer } from "./types/IRowsContainer";
import { trace } from "mobx"
import {getDataViewPropertyById} from "model/selectors/DataView/getDataViewPropertyById";
import {getDataView} from "model/selectors/DataView/getDataView";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { IProperty } from "./types/IProperty";

export class ListRowContainer implements IRowsContainer {
  private orderingConfiguration: IOrderingConfiguration;
  private filterConfiguration: IFilterConfiguration;
  private reactionDisposer: IReactionDisposer | undefined;
  @observable
  private forcedFirstRowId: string | undefined;
  constructor(
    orderingConfiguration: IOrderingConfiguration,
    filterConfiguration: IFilterConfiguration,
    rowIdGetter: (row: any[]) => string,
    parent: any
  ) {
    this.orderingConfiguration = orderingConfiguration;
    this.filterConfiguration = filterConfiguration;
    this.rowIdGetter = rowIdGetter;
    this.parent = parent;
  }

  @observable.shallow allRows: any[][] = [];
  rowIdGetter: (row: any[]) => any;

  @observable
  sortedIds: any[] | undefined;

  @computed get idToRow() {
    const entries = this.allRows.map(row => [this.rowIdGetter(row), row] as [any, any[]]);
    return new Map<any, any[]>(entries);
  }

  async start() {
    this.reactionDisposer = reaction(
      () => [
        this.filterConfiguration.activeFilters.map((filter) => [
          filter.propertyId,
          filter.setting.val1,
          filter.setting.val2,
          filter.setting.type,
        ]),
        this.orderingConfiguration.orderings],
      () => this.updateSortAndFilter(),
    );
    await this.updateSortAndFilter();
  }

  stop(){
    this.reactionDisposer?.();
  }

  *preloadLookups(){
    const dataView = getDataView(this.orderingConfiguration);
    const dataTable = getDataTable(dataView);

    const orderingComboProps = this.orderingConfiguration.userOrderings
      .map((term) => getDataViewPropertyById(this.orderingConfiguration, term.columnId)!)
      .filter((prop) => prop.column === "ComboBox");

    const filterComboProps = this.filterConfiguration.activeFilters
      .map((term) => getDataViewPropertyById(this.filterConfiguration, term.propertyId)!)
      .filter((prop) => prop.column === "ComboBox");
    const allcomboProps = Array.from(new Set(filterComboProps.concat(orderingComboProps)));

    yield Promise.all(
      allcomboProps.map(async (prop) => {
        return prop.lookupEngine?.lookupResolver.resolveList(
          new Set(dataTable.getAllValuesOfProp(prop))
        );
      })
    );
  }

  @action
  async updateSortAndFilter() {
    const self = this;
    await flow(
      function* () {
        yield * self.preloadLookups();
        let rows = self.allRows;
        if (self.filterConfiguration.filteringFunction) {
          rows = rows.filter((row) => self.filterConfiguration.filteringFunction()(row));
        }
        if (self.orderingConfiguration.orderings.length !== 0) {
          rows = rows.sort((row1: any[], row2: any[]) => self.internalRowOrderingFunc(row1, row2));
        }
        self.sortedIds = rows.map(row => self.rowIdGetter(row));
      }
    )();
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
      this.updateSortAndFilter();
    }
  }

  async insert(index: number, row: any[]): Promise<any> {
    this.allRows.splice(index, 0, row);
    this.forcedFirstRowId = this.rowIdGetter(row);
    await this.updateSortAndFilter();
  }

  set(rows: any[][]) {
    this.clear();
    for(let row of rows) this.allRows.push(row);
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

  parent: any;

  get addedRowPositionLocked(): boolean {
    return this.forcedFirstRowId !== undefined;
  }

  getFirstRow(): any[] | undefined {
    if (this.rows.length === 0) {
      return undefined;
    }
    return this.addedRowPositionLocked
      ? this.rows.find(row => this.rowIdGetter(row) === this.forcedFirstRowId)
      : this.rows[0];
  }
}
