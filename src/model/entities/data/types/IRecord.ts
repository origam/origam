import { ICellValue } from "./ICellValue";
import { IRecordId } from "../../values/types/IRecordId";
import { IPropertyId } from "../../values/types/IPropertyId";

export interface IRecord {
  id: IRecordId;
  values: ICellValue[];
  dirtyValues: Map<IRecordId, ICellValue>;

  getValueByIndex(idx: number): ICellValue;
  getDirtyValueByKey(key: IPropertyId): ICellValue;
  hasDirtyValue(key: IPropertyId): boolean;
  setValues(values: ICellValue[]): void;
  setDirtyValue(key: IPropertyId, value: ICellValue): void;
}
