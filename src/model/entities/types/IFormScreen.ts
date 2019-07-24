import { IDataView } from "./IDataView";
import { IDataSource } from "./IDataSource";
import { IComponentBinding } from "./IComponentBinding";
import { IFormScreenLifecycle } from "./IFormScreenLifecycle";
import { IAction } from "./IAction";

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
  isSessioned: boolean;
  // componentBindings: types.array(ComponentBinding),
  // dataSources: types.array(DataSource)
}

export interface ILoadedFormScreen extends ILoadedFormScreenData {
  $type_ILoadedFormScreen: 1;

  isLoading: false;
  rootDataViews: IDataView[];

  getBindingsByChildId(childId: string): IComponentBinding[];
  getBindingsByParentId(parentId: string): IComponentBinding[];
  getDataViewByModelInstanceId(modelInstanceId: string): IDataView | undefined;
  getDataViewByEntity(entity: string): IDataView | undefined;
  getDataSourceByEntity(entity: string): IDataSource | undefined;

  toolbarActions: Array<{ section: string; actions: IAction[] }>;

  parent?: any;
}

export interface ILoadingFormScreenData {
  formScreenLifecycle: IFormScreenLifecycle;
}

export interface ILoadingFormScreen extends ILoadingFormScreenData {
  $type_ILoadingFormScreen: 1;

  isLoading: true;
  parent?: any;

  run(): void;
}

export type IFormScreen = ILoadingFormScreen | ILoadedFormScreen;

export const isILoadingFormScreen = (o: any): o is ILoadingFormScreen =>
  o.$type_ILoadingFormScreen;
export const isILoadedFormScreen = (o: any): o is ILoadedFormScreen =>
  o.$type_ILoadedFormScreen;
