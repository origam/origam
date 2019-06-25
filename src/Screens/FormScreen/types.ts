import { IScreenType, IMainView } from "../types";
import { IDataViewMediator02 } from "../../DataView/DataViewMediator02";
import { IDispatcher } from "../../utils/mediator";
import { IApi } from "../../Api/IApi";

export interface IFormScreen extends IMainView {
  type: IScreenType.FormRef;
  isSessioned: boolean;
  isDirty: boolean;
  setDirty(state: boolean): void;
  sessionId: string;
  title: string;
  isLoading: boolean;
  isVisible: boolean;
  uiStructure: any;
  dataViewMap: Map<string, IDataViewMediator02>;
  api: IApi;
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
  send(event: any): void;
}

export interface IScreenContentFactory {
  create(
    xmlObj: any
  ): {
    screenUI: any;
    dataViews: any;
    isSessioned: boolean;
  };
}

export interface IFormScreenMediator {}

export interface IFormScreenDispatcher {}
