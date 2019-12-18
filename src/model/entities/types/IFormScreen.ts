import { IDataView } from "./IDataView";
import { IDataSource } from "./IDataSource";
import { IComponentBinding } from "./IComponentBinding";
import {
  IFormScreenLifecycle,
  IFormScreenLifecycle02
} from "./IFormScreenLifecycle";
import { IAction } from "./IAction";

/*
export interface ILoadedFormScreenData {
  title: string;
  menuId: string;
  openingOrder: number;
  showInfoPanel: boolean;
  autoRefreshInterval: number;
  cacheOnClient: boolean;
  autoSaveOnListRecordChange: boolean;
  requestSaveAfterUpdate: boolean;
  dataViews: IDataView[];
  dataSources: IDataSource[];
  componentBindings: IComponentBinding[];
  screenUI: any;
  formScreenLifecycle: IFormScreenLifecycle;
  sessionId: string;
}

export interface ILoadedFormScreen extends ILoadedFormScreenData {
  $type_ILoadedFormScreen: 1;

  isDirty: boolean;

  isLoading: false;
  rootDataViews: IDataView[];
  dontRequestData: boolean;

  getBindingsByChildId(childId: string): IComponentBinding[];
  getBindingsByParentId(parentId: string): IComponentBinding[];
  getDataViewByModelInstanceId(modelInstanceId: string): IDataView | undefined;
  getDataViewsByEntity(entity: string): IDataView[];
  getDataSourceByEntity(entity: string): IDataSource | undefined;

  toolbarActions: Array<{ section: string; actions: IAction[] }>;
  dialogActions: IAction[];

  setDirty(state: boolean): void;

  printMasterDetailTree(): void;

  parent?: any;
}

export interface ILoadingFormScreenData {
  formScreenLifecycle: IFormScreenLifecycle;
}

export interface ILoadingFormScreen extends ILoadingFormScreenData {
  $type_ILoadingFormScreen: 1;

  isLoading: true;
  parent?: any;

  start(): void;
}

export type IFormScreen = ILoadingFormScreen | ILoadedFormScreen;

export const isILoadingFormScreen = (o: any): o is ILoadingFormScreen =>
  o.$type_ILoadingFormScreen;
export const isILoadedFormScreen = (o: any): o is ILoadedFormScreen =>
  o.$type_ILoadedFormScreen; */

export interface IFormScreenEnvelopeData {
  formScreenLifecycle: IFormScreenLifecycle02;
}

export interface IFormScreenEnvelope extends IFormScreenEnvelopeData {
  $type_IFormScreenEnvelope: 1;

  isLoading: boolean;
  formScreen?: IFormScreen;

  setFormScreen(formScreen?: IFormScreen): void;
  start(initUIResult: any): Generator;

  parent?: any;
}

export interface IFormScreenData {
  title: string;
  menuId: string;
  openingOrder: number;
  showInfoPanel: boolean;
  autoRefreshInterval: number;
  cacheOnClient: boolean;
  autoSaveOnListRecordChange: boolean;
  requestSaveAfterUpdate: boolean;
  dataViews: IDataView[];
  dataSources: IDataSource[];
  componentBindings: IComponentBinding[];
  screenUI: any;
  panelConfigurations: Map<string, { position: number | undefined }>;
  formScreenLifecycle: IFormScreenLifecycle02;
  sessionId: string;
}

export interface IFormScreen extends IFormScreenData {
  $type_IFormScreen: 1;

  isDirty: boolean;

  isLoading: false;
  rootDataViews: IDataView[];
  dontRequestData: boolean;
  toolbarActions: Array<{ section: string; actions: IAction[] }>;
  dialogActions: IAction[];

  getPanelPosition(id: string): number | undefined;

  getBindingsByChildId(childId: string): IComponentBinding[];
  getBindingsByParentId(parentId: string): IComponentBinding[];
  getDataViewByModelInstanceId(modelInstanceId: string): IDataView | undefined;
  getDataViewsByEntity(entity: string): IDataView[];
  getDataSourceByEntity(entity: string): IDataSource | undefined;

  setDirty(state: boolean): void;
  printMasterDetailTree(): void;

  parent?: any;
}

export const isIFormScreenEnvelope = (o: any): o is IFormScreenEnvelope =>
  o.$type_IFormScreenEnvelope;

export const isIFormScreen = (o: any): o is IFormScreen => o.$type_IFormScreen;
