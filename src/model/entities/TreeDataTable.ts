import {IDataTable} from "./types/IDataTable";
import {IProperty} from "./types/IProperty";
import {IRowsContainer} from "./types/IRowsContainer";
import {IAdditionalRowData} from "./types/IAdditionalRecordData";
import {IGroupTreeNode} from "../../gui/Components/ScreenElements/Table/TableRendering/types";
import {IOrderingConfiguration} from "./types/IOrderingConfiguration";
import {IFilterConfiguration} from "./types/IFilterConfiguration";
import {ListRowContainer} from "./ListRowContainer";
import {IDataSourceField} from "./types/IDataSourceField";
import {computed} from "mobx";
import {getDataSource} from "../selectors/DataSources/getDataSource";

export class TreeDataTable implements IDataTable {
  $type_IDataTable: 1 = 1;
  properties: IProperty[] = [];
  rowsContainer: IRowsContainer;

  get rows() {
    return this.rowsContainer.rows;
  }

  get allRows(): any[][] {
    return this.rows;
  }

  additionalRowData: Map<string, IAdditionalRowData> = new Map();
  maxRowCountSeen: number = 0;
  groups: IGroupTreeNode[] = [];
  private parentIdProperty: string;
  isEmpty: boolean = false;
  idProperty: string;

  constructor(idProperty: string, parentIdProperty: string, orderingConfiguration: IOrderingConfiguration,
              filterConfiguration: IFilterConfiguration) {
    this.parentIdProperty = parentIdProperty;
    this.idProperty = idProperty;
    this.rowsContainer = new ListRowContainer(orderingConfiguration, filterConfiguration, (row: any[]) => this.getRowId(row));
  }

  getRowId(row: any[]): string {
    return row[this.dataSource.getFieldByName(this.idProperty)!.index];
  }

  getCellValue(row: any[], property: IProperty): any {
    throw new Error("Not implemented");
  }

  getCellValueByDataSourceField(row: any[], dsField: IDataSourceField) {
    return row[dsField.index];
  }

  getCellText(row: any[], property: IProperty): any {
    throw new Error("Not implemented");
  }

  resolveCellText(property: IProperty, value: any): any {
    throw new Error("Not implemented");
  }

  getRowByExistingIdx(idx: number): any[] {
    return this.rows[idx];
  }

  getRowById(id: string): any[] | undefined {
    throw new Error("Not implemented");
  }

  getExistingRowIdxById(id: string): number | undefined {
    for (let i = 0; i < this.rows.length; i++) {
      if (this.getRowId(this.rows[i]) === id) {
        return i;
      }
    }
  }

  getPropertyById(id: string): IProperty | undefined {
    return undefined;
  }

  getFirstRow(): any[] | undefined {
    if (this.rows.length > 0) {
      return this.rows[0];
    } else {
      return undefined;
    }
  }

  getNearestRow(row: any[]): any[] | undefined {
    throw new Error("Not implemented");
  }

  getNextExistingRowId(id: string): string | undefined {
    throw new Error("Not implemented");
  }

  getPrevExistingRowId(id: string): string | undefined {
    throw new Error("Not implemented");
  }

  getDirtyValues(row: any[]): Map<string, any> {
    throw new Error("Not implemented");
  }

  getDirtyValueRows(): any[][] {
    throw new Error("Not implemented");
  }

  getDirtyDeletedRows(): any[][] {
    throw new Error("Not implemented");
  }

  getDirtyNewRows(): any[][] {
    throw new Error("Not implemented");
  }

  getAllValuesOfProp(property: IProperty): any[] {
    throw new Error("Not implemented");
  }

  setRecords(rows: any[][]): void {
    this.rowsContainer.set(rows);
  }

  setFormDirtyValue(row: any[], propertyId: string, value: any): void {
    throw new Error("Not implemented");
  }

  setDirtyValue(row: any[], columnId: string, value: any): void {
    throw new Error("Not implemented");
  }

  flushFormToTable(row: any[]): void {
    throw new Error("Not implemented");
  }

  setDirtyDeleted(row: any[]): void {
    throw new Error("Not implemented");
  }

  setDirtyNew(row: any[]): void {
    throw new Error("Not implemented");
  }

  deleteAdditionalRowData(row: any[]): void {
    throw new Error("Not implemented");
  }

  deleteRow(row: any[]): void {
    throw new Error("Not implemented");
  }

  clear(): void {
    throw new Error("Not implemented");
  }

  clearRecordDirtyValues(id: string): void {
    throw new Error("Not implemented");
  }

  substituteRecord(row: any[]): void {
    throw new Error("Not implemented");
  }

  insertRecord(index: number, row: any[]): void {
    throw new Error("Not implemented");
  }

  @computed get dataSource() {
    return getDataSource(this);
  }

  parent?: any;

  getLastRow(): any[] | undefined {
    return undefined;
  }
}