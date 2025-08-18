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

import { TypeSymbol } from "dic/Container";
import { action, computed, observable } from "mobx";
import { EagerlyLoadedGrid, LazilyLoadedGrid } from "./DropdownEditorCommon";
import { IDropdownEditorData } from "./DropdownEditorData";
import { DropdownEditorSetup } from "modules/Editors/DropdownEditor/DropdownEditorSetup";

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

  @computed
  get columnIdsWithNoData() {
    const result: string[] = [];
    outer: for (let column of this.setup().visibleColumnNames) {
      for (let i = 0; i < this.allRows.length; i++) {
        if (this.allRows[i][this.setup().columnNameToIndex.get(column)!] !== null) {
          continue outer;
        }
      }
      result.push(column);
    }
    return result;
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

  formattedText(i: number): string;
}

export interface IDropdownColumnDriver {
  columnId: string;
  headerCellDriver: IHeaderCellDriver;
  bodyCellDriver: IBodyCellDriver;
}

export class DropdownColumnDrivers {
  @observable
  allDrivers: IDropdownColumnDriver[] = [];

  customFieldStyle: { [key: string]: string } | undefined;

  driversFilter = (driver: IDropdownColumnDriver) => {
    return true;
  };

  @computed
  get drivers() {
    return this.allDrivers.filter((driver) => {
      return this.driversFilter(driver);
    });
  }

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

  constructor(rows: any[][]) {
    if (rows.length > 0) {
      this.items = rows.map((row) => new IndexItem(row));
    }
  }

  search(phrase: string) {
    const phraseLower = phrase.toLowerCase();
    return this.items.filter((item) => item.matches(phraseLower)).map((item) => item.row);
  }
}

class IndexItem {
  private textInLower: string;
  public row: any[];

  constructor(row: any[]) {
    this.row = row;
    this.textInLower = row.slice(1).join("").toLowerCase();
  }

  matches(phraseInLower: string) {
    return this.textInLower.includes(phraseInLower);
  }
}
