import { IPropertyId } from '../../values/types/IPropertyId';
import { ILookup } from './ILookup';
import { IRecord } from './IRecord';

export interface IProperty {
  id: IPropertyId;
  name: string;
  entity: string;
  column: string;
  isReadOnly: boolean;
  recordDataIndex: number;
  lookup: ILookup | undefined;
}