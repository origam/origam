import { IScreenType, IMainView } from "../types";
import { IDataView } from "../../DataView/types/IDataView";

export interface IFormScreen extends IMainView {
  type: IScreenType.FormRef;
  title: string;
  isLoading: boolean;
  isVisible: boolean;
  uiStructure: any;
  dataViewMap: Map<string, IDataView>;
  setUIStructure(uiStructure: any): void;
  setDataViews(views: IDataView[]): void;
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