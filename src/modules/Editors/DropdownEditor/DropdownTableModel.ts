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
  ) {}

  @observable.shallow allRows: any[][] = [];
  @observable filterPhrase: string = "";
  fulltext: any;

  get rowsBeforeRedundancyFilter(): any[][] {
    const setup = this.setup();
    switch (setup.dropdownType) {
      case EagerlyLoadedGrid: {
        if (this.filterPhrase !== "" && this.fulltext) {
          return this.fulltext.search(this.filterPhrase);
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
    const setup = this.setup();
    switch (setup.dropdownType) {
      case EagerlyLoadedGrid: {
        this.fulltext = this.createFulltext(rows);
        break;
      }
    }
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

  createFulltext(rows: any[][]) {
    const flexFields: string[] = [];
    const setup = this.setup();
    const isOnlyFirstColumn = setup.searchByFirstColumnOnly;
    for (
      let i = 0;
      i <
      (isOnlyFirstColumn
        ? Math.min(1, setup.visibleColumnNames.length)
        : setup.visibleColumnNames.length);
      i++
    ) {
      flexFields.push(`${setup.columnNameToIndex.get(setup.visibleColumnNames[i])}`);
    }
    const flexIndex = FlexSearch.create({
      encode: "icase",
      doc: {
        id: `${setup.identifierIndex}`,
        field: flexFields,
      },
    });

    function arr2obj(arr: any[]) {
      const result: { [k: string]: any } = {};
      for (let i = 0; i < arr.length; i++) {
        result[`${i}`] = arr[i];
      }
      return result;
    }

    for (let i = 0; i < rows.length; i++) {
      flexIndex.add(arr2obj(rows[i]));
    }
    return flexIndex;
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
