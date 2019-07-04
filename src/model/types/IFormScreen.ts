import { IDataView } from "./IDataView";
import { IDataSource } from "./IDataSource";
import { IComponentBinding } from "./IComponentBinding";

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
  // componentBindings: types.array(ComponentBinding),
  // dataSources: types.array(DataSource)
}

export interface ILoadedFormScreen extends ILoadedFormScreenData {
  parent?: any;
}

export interface ILoadingFormScreenData {}

export interface ILoadingFormScreen extends ILoadingFormScreenData {
  parent?: any;
}

export type IFormScreen = ILoadingFormScreen | ILoadedFormScreen;
