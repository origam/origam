import {IViewType} from '../../../specificViews/types/IViewType';
import { ICursor } from 'src/model/entities/cursor/types/ICursor';
import { IDataTable } from 'src/model/entities/data/types/IDataTable';
import { IProperties } from 'src/model/entities/data/types/IProperties';

export interface IFormView {
  type: IViewType.FormView;
  id: string;
  cursor: ICursor;
  dataTable: IDataTable;
  properties: IProperties;
  reorderedProperties: IProperties;
}