import { IRecord } from '../../../data/types/IRecord';
import { IRecordId } from '../../../values/types/IRecordId';
import { IPropertyId } from '../../../values/types/IPropertyId';

export interface IDataTable {
  getNewRecord(): IRecord;
  getRecordIdAfterId(recordId: IRecordId): IRecordId | undefined;
  getRecordIdBeforeId(recordId: IRecordId): IRecordId | undefined;
  getPropertyIdAfterId(propertyId: IPropertyId): IPropertyId | undefined;
  getPropertyIdBeforeId(propertyId: IPropertyId): IPropertyId | undefined;
  insertRecordAfterId(rowId: IRecordId, record: IRecord): void;
  insertRecordBeforeId(rowId: IRecordId, record: IRecord): void;
  deleteRecordById(rowId: IRecordId): void;
  copyRecordById(rowId: IRecordId): IRecord;
}