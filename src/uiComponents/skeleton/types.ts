import { IProperty } from "src/screenInterpreter/types";

export interface IDataViewProps {
  hasGridView?: boolean;
  hasFormView?: boolean;
  hasMapView?: boolean;
  properties: IProperty[];
  entity: string;
  isHeadless: boolean;
  isActionButtonsDisabled: boolean;
  isShowAddButton: boolean;
  isShowDeleteButton: boolean;
  initialView: IDataViewType;
}

export enum IDataViewType {
  Form = "Form",
  Table = "Table",
  Map = "Map"
}

export const IDataViewOrder = {
  "0": IDataViewType.Form,
  "1": IDataViewType.Table,
  "5": IDataViewType.Map
};

export interface IDataViewState {
  activeView: IDataViewType;
  setActiveView(view: IDataViewType): void;
}
