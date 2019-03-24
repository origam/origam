import { IFormView } from "./types/IFormView";
import { IProperties } from "../../data/types/IProperties";
import { ICursor } from "../../cursor/types/ICursor";
import { IDataTable } from "../table/types/IDataTable";
import { IViewType } from 'src/model/entities/specificViews/types/IViewType';


export interface IFormViewParam {
  cursor: ICursor;
  dataTable: IDataTable;
  properties: IProperties;
}

export class FormView implements IFormView {
  constructor(param: IFormViewParam) {
    this.cursor = param.cursor;
    this.dataTable = param.dataTable;
    this.properties = param.properties;
  }

  type: IViewType.FormView = IViewType.FormView;
  cursor: ICursor;
  dataTable: IDataTable;
  properties: IProperties;
}
