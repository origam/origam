import { IFormView } from "./types/IFormView";
import { IProperties } from "../../data/types/IProperties";
import { ICursor } from "../../cursor/types/ICursor";
import { IViewType } from 'src/model/entities/specificViews/types/IViewType';
import { IDataTable } from "../../data/types/IDataTable";


export interface IFormViewParam {
  id: string;
  cursor: ICursor;
  dataTable: IDataTable;
  properties: IProperties;
  reorderedProperties: IProperties;
}

export class FormView implements IFormView {
  constructor(param: IFormViewParam) {
    this.id = param.id;
    this.cursor = param.cursor;
    this.dataTable = param.dataTable;
    this.properties = param.properties;
    this.reorderedProperties = param.reorderedProperties;
  }

  type: IViewType.FormView = IViewType.FormView;
  id: string;
  cursor: ICursor;
  dataTable: IDataTable;
  properties: IProperties;
  reorderedProperties: IProperties;
}
