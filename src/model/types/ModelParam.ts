import { IViewType } from "../entities/specificViews/types/IViewType";


export interface IDataViewParam {
  id: string;
  initialView: IViewType;
  properties: IPropertyParam[];
}

export interface IPropertyParam {
  id: string;
  name: string;
  entity: string;
  column: string;
  isReadOnly: boolean;
}
