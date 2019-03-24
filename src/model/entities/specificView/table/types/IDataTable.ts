import { IRecord } from '../../../data/types/IRecord';
import { IRecordId } from '../../../values/types/IRecordId';
import { IPropertyId } from '../../../values/types/IPropertyId';

export interface IDataTable {
  getNewRecord(): IRecord;
  getRecordIdAfterId(recordId: IRecordId): IRecordId;
  getRecordIdBeforeId(recordId: IRecordId): IRecordId;
  getPropertyIdAfterId(propertyId: IPropertyId): IPropertyId;
  getPropertyIdBeforeId(propertyId: IPropertyId): IPropertyId;
  insertRecordAfterId(rowId: IRecordId, record: IRecord): void;
  insertRecordBeforeId(rowId: IRecordId, record: IRecord): void;
  deleteRecordById(rowId: IRecordId): void;
  copyRecordById(rowId: IRecordId): IRecord;
}