import { IDataView } from "./types/IDataView";
import { IDataSource } from "./types/IDataSource";
import { IComponentBinding } from "./types/IComponentBinding";
import {
  ILoadedFormScreenData,
  ILoadedFormScreen,
  CFormScreen,
  ILoadingFormScreen
} from "./types/IFormScreen";
import { IFormScreenLifecycle } from "./types/IFormScreenLifecycle";
import { ILoadingFormScreenData } from "./types/IFormScreen";

export class FormScreen implements ILoadedFormScreen {
  
  $type: typeof CFormScreen = CFormScreen;

  constructor(data: ILoadedFormScreenData) {
    Object.assign(this, data);
    this.formScreenLifecycle.parent = this;
    this.dataViews.forEach(o => (o.parent = this));
    this.dataSources.forEach(o => (o.parent = this));
    this.componentBindings.forEach(o => (o.parent = this));
  }

  parent: any;

  title: string = "";
  menuId: string = "";
  openingOrder: number = 0;
  showInfoPanel: boolean = false;
  autoRefreshInterval: number = 0;
  cacheOnClient: boolean = false;
  autoSaveOnListRecordChange: boolean = false;
  requestSaveAfterUpdate: boolean = false;
  screenUI: any;
  isLoading: false = false;
  formScreenLifecycle: IFormScreenLifecycle = null as any;
  dataViews: IDataView[] = [];
  dataSources: IDataSource[] = [];
  componentBindings: IComponentBinding[] = [];
}

export class LoadingFormScreen implements ILoadingFormScreen {

  $type: typeof CFormScreen = CFormScreen;

  constructor(data: ILoadingFormScreenData) {
    Object.assign(this, data);
    this.formScreenLifecycle.parent = this;
  }

  isLoading: true = true;
  formScreenLifecycle: IFormScreenLifecycle = null as any;

  parent?: any;

  run(): void {
    this.formScreenLifecycle.run();
  }
}
