import { IDataView } from "./IDataView";
import { IDataSource } from "./IDataSource";
import { IComponentBinding } from "./IComponentBinding";
import { IFormScreenLifecycle } from "./IFormScreenLifecycle";

export const CFormScreen = "CFormScreen";

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
  // componentBindings: types.array(ComponentBinding),
  // dataSources: types.array(DataSource)
}

export interface ILoadedFormScreen extends ILoadedFormScreenData {
  $type: typeof CFormScreen;
  isLoading: false;
  parent?: any;
}

export interface ILoadingFormScreenData {
  formScreenLifecycle: IFormScreenLifecycle;
}

export interface ILoadingFormScreen extends ILoadingFormScreenData {
  $type: typeof CFormScreen;

  isLoading: true;
  parent?: any;

  run(): void;
}

export type IFormScreen = ILoadingFormScreen | ILoadedFormScreen;
