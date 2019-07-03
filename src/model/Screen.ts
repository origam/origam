import { IScreen, IScreenData } from "./types/IScreen";
import { IDataView } from "./types/IDataView";
import { IDataSource } from "./types/IDataSource";
import { IComponentBinding } from "./types/IComponentBinding";

export class Screen implements IScreen {
  constructor(data: IScreenData) {
    Object.assign(this, data);
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
  dataViews: IDataView[] = [];
  dataSources: IDataSource[] = [];
  componentBindings: IComponentBinding[] = [];
}
