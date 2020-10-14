import { TypeSymbol } from "dic/Container";
import FlexSearch from "flexsearch";
import { action, computed, observable } from "mobx";
import { DropdownEditorSetup } from "./DropdownEditor";
import { EagerlyLoadedGrid, LazilyLoadedGrid } from "./DropdownEditorCommon";
import { IDropdownEditorData } from "./DropdownEditorData";

export class DropdownDataTable {
  constructor(
    private setup: () => DropdownEditorSetup,
    private dropdownEditorData: IDropdownEditorData
  ) {
  }

  @observable.shallow allRows: any[][] = [];
  @observable filterPhrase: string = "";

  @computed
  get index() {
    return new Index(this.allRows);
  }

  get rowsBeforeRedundancyFilter(): any[][] {
    const setup = this.setup();
    switch (setup.dropdownType) {
      case EagerlyLoadedGrid: {
        if (this.filterPhrase !== "") {
          return this.index.search(this.filterPhrase);
        }
        return this.allRows;
      }
      default:
      case LazilyLoadedGrid: {
        return this.allRows;
      }
    }
  }

  @computed get rows(): any[][] {
    return this.rowsBeforeRedundancyFilter.filter(
      (row) => !this.dropdownEditorData.idsInEditor.includes(this.getRowIdentifier(row))
    );
  }

  get rowCount() {
    return this.rows.length;
  }

  getValue(dataRowIndex: number, dataColumnIndex: number) {
    return this.rows[dataRowIndex][dataColumnIndex];
  }

  getRowByIndex(rowIndex: number) {
    return this.rows[rowIndex];
  }

  getRowIdentifierByIndex(rowIndex: number) {
    return this.rows[rowIndex][this.setup().identifierIndex];
  }

  getRowIdentifier(row: any[]) {
    return row[this.setup().identifierIndex];
  }

  getRowById(id: string) {
    return this.rows.find((row) => this.getRowIdentifier(row) === id);
  }

  getRowIndexById(id: string) {
    return this.rows.findIndex((row) => this.getRowIdentifier(row) === id);
  }

  getRowAfterId(id: string) {
    const idx = this.getRowIndexById(id);
    if (idx > -1 && idx < this.rowCount - 1) {
      return this.getRowByIndex(idx + 1);
    } else return undefined;
  }

  getRowBeforeId(id: string) {
    const idx = this.getRowIndexById(id);
    if (idx > 0) {
      return this.getRowByIndex(idx - 1);
    } else return undefined;
  }

  getRowIdAfterId(id: string) {
    const row = this.getRowAfterId(id);
    if (row) return this.getRowIdentifier(row);
  }

  getRowIdBeforeId(id: string) {
    const row = this.getRowBeforeId(id);
    if (row) return this.getRowIdentifier(row);
  }

  @action.bound setData(rows: any[][]) {
    this.allRows = rows;
  }

  @action.bound appendData(rows: any[][]) {
    this.allRows.push(...rows);
  }

  @action.bound clearData() {
    this.allRows.length = 0;
  }

  @action.bound setFilterPhrase(phrase: string) {
    this.filterPhrase = phrase;
  }
}

export const IDropdownDataTable = TypeSymbol<DropdownDataTable>("IDropdownDataTable");

export interface IHeaderCellDriver {
  render(): React.ReactNode;
}

export interface IBodyCellDriver {
  render(rowIndex: number): React.ReactNode;
}

export interface IDropdownColumnDriver {
  headerCellDriver: IHeaderCellDriver;
  bodyCellDriver: IBodyCellDriver;
}

export class DropdownColumnDrivers {
  drivers: IDropdownColumnDriver[] = [];

  get driverCount() {
    return this.drivers.length;
  }

  getDriver(columnIndex: number) {
    return this.drivers[columnIndex];
  }
}

export const IDropdownColumnDrivers = TypeSymbol<DropdownColumnDrivers>("IDropdownColumnDrivers");


class Index {

  private items: IndexItem[] = [];

  constructor(rows: any[][]){
    if(rows.length > 0){
      this.items = rows.map(row => new IndexItem(row));
    }
  }
  search(phrase: string){
    const phraseLower = phrase.toLowerCase();
    return this.items.filter(item => item.matches(phraseLower)).map(item => item.row);
  }
}

class IndexItem {
  private textInLower : string;
  public row: any[];
  constructor(row: any[]){
    this.row = row;
    this.textInLower = row.slice(1).join().toLowerCase();
  }

  matches(phraseInLower: string){
    return this.textInLower.includes(phraseInLower);
  }
}