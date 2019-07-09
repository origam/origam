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
import { computed } from "mobx";

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
  isSessioned: boolean = false;
  dataViews: IDataView[] = [];
  dataSources: IDataSource[] = [];
  componentBindings: IComponentBinding[] = [];

  @computed get rootDataViews(): IDataView[] {
    return this.dataViews.filter(dv => dv.isBindingRoot);
  }

  getBindingsByChildId(childId: string) {
    return this.componentBindings.filter(b => b.childId === childId);
  }

  getBindingsByParentId(parentId: string) {
    return this.componentBindings.filter(b => b.parentId === parentId);
  }

  getDataViewByModelInstanceId(modelInstanceId: string): IDataView | undefined {
    return this.dataViews.find(dv => dv.modelInstanceId === modelInstanceId);
  }

  getDataSourceByEntity(entity: string): IDataSource | undefined {
    return this.dataSources.find(ds => ds.entity === entity);
  }
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
