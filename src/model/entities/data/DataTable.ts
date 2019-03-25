import { IDataTable } from "./types/IDataTable";
import { IRecords } from "./types/IRecords";
import { IProperties } from "./types/IProperties";
import { IRecord } from "./types/IRecord";
import { IProperty } from "./types/IProperty";
import { action, computed } from "mobx";
import { IRecordId } from '../values/types/IRecordId';
import { IPropertyId } from "../values/types/IPropertyId";
import { ICellValue } from './types/ICellValue';

interface IDataTableParam {
  records: IRecords;
  properties: IProperties;
}

export class DataTable implements IDataTable {
  
  constructor(param: IDataTableParam) {
    this.records = param.records;
    this.properties = param.properties;
  }

  records: IRecords;
  properties: IProperties;

  get visibleRecordCount(): number {
    return this.records.visibleCount;
  }

  get columnCount(): number {
    return this.properties.count;
  }

  getInitialValue(record: IRecord, property: IProperty) {
    return record.getValueByIndex(property.recordDataIndex);
  }

  getDirtyValue(record: IRecord, property: IProperty) {
    return record.getDirtyValueByKey(property.id);
  }

  getValue(record: IRecord, property: IProperty) {
    return record.hasDirtyValue(property.id)
      ? this.getDirtyValue(record, property)
      : this.getInitialValue(record, property);
  }

  getInitialValueById(recId: string, propId: string) {
    const record = this.records.byId(recId);
    const property = this.properties.byId(propId);
    if (record && property) {
      return this.getInitialValue(record, property);
    }
  }

  getDirtyValueById(recId: string, propId: string) {
    const record = this.records.byId(recId);
    const property = this.properties.byId(propId);
    if (record && property) {
      return this.getDirtyValue(record, property);
    }
  }

  getValueById(recId: string, propId: string) {
    const record = this.records.byId(recId);
    const property = this.properties.byId(propId);
    if (record && property) {
      return this.getValue(record, property);
    }
  }

  getInitialValueByIndex(recIdx: number, propIdx: number) {
    const record = this.records.byIndex(recIdx);
    const property = this.properties.byIndex(propIdx);
    if (record && property) {
      return this.getInitialValue(record, property);
    }
  }

  getDirtyValueByIndex(recIdx: number, propIdx: number) {
    const record = this.records.byIndex(recIdx);
    const property = this.properties.byIndex(propIdx);
    if (record && property) {
      return this.getDirtyValue(record, property);
    }
  }

  getValueByIndex(recIdx: number, propIdx: number) {
    const record = this.records.byIndex(recIdx);
    const property = this.properties.byIndex(propIdx);
    if (record && property) {
      return this.getValue(record, property);
    }
  }

  createRecord(): IRecord {
    throw new Error("Method not implemented.");
  }

  @action.bound
  setDirtyValue(record: IRecord, property: IProperty, value: any): void {
    record.setDirtyValue(property.id, value);
  }

  @action.bound
  setDirtyValueById(recId: IRecordId, propId: IPropertyId, value: ICellValue): void {
    const record = this.records.byId(recId);
    const property = this.properties.byId(propId);
    if (property && record) {
      this.setDirtyValue(record, property, value);
    }
  }

  @action.bound
  setDirtyValueByIndex(recIdx: number, propIdx: number, value: any): void {
    const record = this.records.byIndex(recIdx);
    const property = this.properties.byIndex(propIdx);
    if (record && property) {
      this.setDirtyValue(record, property, value);
    } // TODO: Exception when rec/prop not found?
  }

  getColumnByIndex(idx: number): IProperty | undefined {
    return this.properties.byIndex(idx);
  }

  getNewRecord(): IRecord {
    throw new Error("Method not implemented.");
  }

  getRecordIdAfterId(recordId: string): string {
    throw new Error("Method not implemented.");
  }

  getRecordIdBeforeId(recordId: string): string {
    throw new Error("Method not implemented.");
  }

  getPropertyIdAfterId(propertyId: string): string {
    throw new Error("Method not implemented.");
  }

  getPropertyIdBeforeId(propertyId: string): string {
    throw new Error("Method not implemented.");
  }

  getRowIndexById(id: string): number | undefined {
    return this.records.id2Index(id);
  }

  getRowIdByIndex(idx: number): string | undefined {
    return this.records.index2Id(idx);
  }

  getColumnIndexById(id: string): number | undefined {
    return this.properties.id2Index(id);
  }

  getColumnIdByIndex(idx: number): string | undefined {
    return this.properties.index2Id(idx);
  }  

  @action.bound
  insertRecordAfterId(rowId: string, record: IRecord): void {
    throw new Error("Method not implemented.");
  }

  @action.bound
  insertRecordBeforeId(rowId: string, record: IRecord): void {
    throw new Error("Method not implemented.");
  }

  @action.bound
  deleteRecordById(rowId: string): void {
    const record = this.records.byId(rowId);
    if (record) {
      this.records.deleteRecord(record);
    }
  }
  
  @action.bound
  copyRecordById(rowId: string): IRecord {
    throw new Error("Method not implemented.");
  }
}
