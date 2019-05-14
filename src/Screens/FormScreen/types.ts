import { IScreenType, IMainView } from "../types";
import { IDataViewMediator02 } from "../../DataView/DataViewMediator02";

export interface IFormScreen extends IMainView {
  type: IScreenType.FormRef;
  title: string;
  isLoading: boolean;
  isVisible: boolean;
  uiStructure: any;
  dataViewMap: Map<string, IDataViewMediator02>;
  setUIStructure(uiStructure: any): void;
  setDataViews(views: IDataViewMediator02[]): void;
  activateDataViews(): void;
}

export function isFormScreen(obj: any): obj is IFormScreen {
  return obj.type === IScreenType.FormRef;
}

export interface IFormScreenMachine {
  start(): void;
  stop(): void;
}

export interface IScreenContentFactory {
  create(xmlObj: any): {
    screenUI: any;
    dataViews: any
  }
}