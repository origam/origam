import { IDataView } from "./IDataView";
import { IDataSource } from "./IDataSource";
import { IComponentBinding } from "./IComponentBinding";
import { IFormScreenLifecycle02 } from "./IFormScreenLifecycle";
import { IAction } from "./IAction";
import { IRefreshOnReturnType } from "../WorkbenchLifecycle/WorkbenchLifecycle";
import { IPanelConfiguration } from "./IPanelConfiguration";
import { IOrdering } from "./IOrderingConfiguration";

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
  preloadedSessionId?: string;
  refreshOnReturnType?: IRefreshOnReturnType;
}

export interface IFormScreenEnvelope extends IFormScreenEnvelopeData {
  $type_IFormScreenEnvelope: 1;

  isLoading: boolean;
  formScreen?: IFormScreen;

  setFormScreen(formScreen?: IFormScreen): void;
  start(initUIResult: any, preloadIsDirty?: boolean): Generator;

  parent?: any;
}

export interface IFormScreenData {
  title: string;
  menuId: string;
  dynamicTitleSource: string | undefined;
  openingOrder: number;
  showInfoPanel: boolean;
  showWorkflowCancelButton: boolean;
  showWorkflowNextButton: boolean;
  autoRefreshInterval: number;
  refreshOnFocus: boolean;
  cacheOnClient: boolean;
  suppressSave: boolean;
  autoSaveOnListRecordChange: boolean;
  requestSaveAfterUpdate: boolean;
  dataViews: IDataView[];
  dataSources: IDataSource[];
  componentBindings: IComponentBinding[];
  screenUI: any;
  panelConfigurations: Map<string, IPanelConfiguration>;
  formScreenLifecycle: IFormScreenLifecycle02;
  sessionId: string;
}

export interface IFormScreen extends IFormScreenData {
  $type_IFormScreen: 1;

  isDirty: boolean;

  isLoading: false;
  rootDataViews: IDataView[];
  nonRootDataViews: IDataView[];
  dontRequestData: boolean;
  toolbarActions: Array<{ section: string; actions: IAction[] }>;
  dialogActions: IAction[];
  dynamicTitle: string | undefined;

  getPanelPosition(id: string): number | undefined;

  getBindingsByChildId(childId: string): IComponentBinding[];
  getBindingsByParentId(parentId: string): IComponentBinding[];
  getDataViewByModelInstanceId(modelInstanceId: string): IDataView | undefined;
  getDataViewsByEntity(entity: string): IDataView[];
  getDataSourceByEntity(entity: string): IDataSource | undefined;

  setDirty(state: boolean): void;
  setTitle(title: string): void;
  printMasterDetailTree(): void;

  parent?: any;
}

export const isIFormScreenEnvelope = (o: any): o is IFormScreenEnvelope =>
  o.$type_IFormScreenEnvelope;

export const isIFormScreen = (o: any): o is IFormScreen => o.$type_IFormScreen;
