import { IRecords } from "./IRecords";
import { IProperties } from './IProperties';
import { ICellValue } from "./ICellValue";
import { IProperty } from "./IProperty";
import { IRecord } from "./IRecord";
import { IRecordId } from '../../values/types/IRecordId';
import { IPropertyId } from '../../values/types/IPropertyId';
import {IDataTable as ITableDataTable} from '../../specificView/table/types/IDataTable';

export interface IDataTable extends ITableDataTable {
  records: IRecords;
  properties: IProperties;

  columnCount: number;
  visibleRecordCount: number;

  getInitialValue(record: IRecord, property: IProperty): ICellValue;
  getDirtyValue(record: IRecord, property: IProperty): ICellValue | undefined;
  getValue(record: IRecord, property: IProperty): ICellValue;

  getInitialValueById(recId: IRecordId, propId: IPropertyId): ICellValue;
  getDirtyValueById(recId: IRecordId, propId: IPropertyId): ICellValue | undefined;
  getValueById(recId: IRecordId, propId: IPropertyId): ICellValue;

  getInitialValueByIndex(recIdx: number, propIdx: number): ICellValue;
  getDirtyValueByIndex(recIdx: number, propIdx: number): ICellValue | undefined;
  getValueByIndex(recIdx: number, propIdx: number): ICellValue;

  setDirtyValue(record: IRecord, property: IProperty, value: ICellValue): void;
  setDirtyValueById(recId: IRecordId, propId: IPropertyId, value: ICellValue): void;
  setDirtyValueByIndex(recIdx: number, propIdx: number, value: ICellValue): void;

  getRowIndexById(id: string): number | undefined;
  getRowIdByIndex(idx: number): string | undefined;
  getColumnIndexById(id: string): number | undefined;
  getColumnIdByIndex(idx: number): string | undefined;

  getColumnByIndex(idx: number): IProperty | undefined;

  createRecord(): IRecord;
}