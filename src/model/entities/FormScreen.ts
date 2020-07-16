import {IDataView} from "./types/IDataView";
import {IDataSource} from "./types/IDataSource";
import {IComponentBinding} from "./types/IComponentBinding";
import {IFormScreenLifecycle02} from "./types/IFormScreenLifecycle";
import {action, computed, observable} from "mobx";
import {IAction} from "./types/IAction";
import {getDontRequestData} from "model/selectors/getDontRequestData";
import {IFormScreen, IFormScreenData, IFormScreenEnvelope, IFormScreenEnvelopeData,} from "./types/IFormScreen";
import {IPanelConfiguration} from "./types/IPanelConfiguration";
import { CriticalSection } from "utils/sync";

export class FormScreen implements IFormScreen {
  $type_IFormScreen: 1 = 1;

  constructor(data: IFormScreenData) {
    Object.assign(this, data);
    this.formScreenLifecycle.parent = this;
    this.dataViews.forEach((o) => (o.parent = this));
    this.dataSources.forEach((o) => (o.parent = this));
    this.componentBindings.forEach((o) => (o.parent = this));
  }

  parent?: any;

  dataUpdateCRS = new CriticalSection();

  @observable isDirty: boolean = false;
  dynamicTitleSource: string | undefined;
  sessionId: string = "";
  @observable title: string = "";
  suppressSave: boolean = false;
  menuId: string = "";
  openingOrder: number = 0;
  showInfoPanel: boolean = false;
  showWorkflowCancelButton: boolean = false;
  showWorkflowNextButton: boolean = false;
  autoRefreshInterval: number = 0;
  refreshOnFocus: boolean = false;
  cacheOnClient: boolean = false;
  autoSaveOnListRecordChange: boolean = false;
  requestSaveAfterUpdate: boolean = false;
  screenUI: any;
  panelConfigurations: Map<string, IPanelConfiguration> = new Map();
  isLoading: false = false;
  formScreenLifecycle: IFormScreenLifecycle02 = null as any;

  dataViews: IDataView[] = [];
  dataSources: IDataSource[] = [];
  componentBindings: IComponentBinding[] = [];

  get dynamicTitle() {
    if (!this.dynamicTitleSource) {
      return undefined;
    }
    const splitSource = this.dynamicTitleSource.split(".");
    const dataSourceName = splitSource[0];
    const columnName = splitSource[1];

    const dataView = this.dataViews.find((view) => view.name === dataSourceName);
    if (!dataView) return undefined;
    const dataSource = this.dataSources.find((view) => view.entity === dataSourceName);
    if (!dataSource) return undefined;
    const dataSourceField = dataSource!.getFieldByName(columnName);
    const dataTable = dataView!.dataTable;

    return dataTable.getCellValueByDataSourceField(dataTable.rows[0], dataSourceField!);
  }

  @computed get dontRequestData() {
    return getDontRequestData(this);
  }

  @computed get rootDataViews(): IDataView[] {
    return this.dataViews.filter((dv) => dv.isBindingRoot);
  }

  @computed get nonRootDataViews(): IDataView[] {
    return this.dataViews.filter((dv) => !dv.isBindingRoot);
  }

  @action.bound setTitle(title: string) {
    this.title = title;
  }

  getBindingsByChildId(childId: string) {
    return this.componentBindings.filter((b) => b.childId === childId);
  }

  getBindingsByParentId(parentId: string) {
    return this.componentBindings.filter((b) => b.parentId === parentId);
  }

  getDataViewByModelInstanceId(modelInstanceId: string): IDataView | undefined {
    return this.dataViews.find((dv) => dv.modelInstanceId === modelInstanceId);
  }

  getDataViewsByEntity(entity: string): IDataView[] {
    return this.dataViews.filter((dv) => dv.entity === entity);
  }

  getDataSourceByEntity(entity: string): IDataSource | undefined {
    return this.dataSources.find((ds) => ds.entity === entity);
  }

  @computed get toolbarActions() {
    const result: Array<{ section: string; actions: IAction[] }> = [];
    for (let dv of this.dataViews) {
      if (dv.toolbarActions.length > 0) {
        result.push({
          section: dv.name,
          actions: dv.toolbarActions,
        });
      }
    }
    return result;
  }

  @computed get dialogActions() {
    const result: IAction[] = [];
    for (let dv of this.dataViews) {
      result.push(...dv.dialogActions);
    }
    return result;
  }

  getPanelPosition(id: string): number | undefined {
    const conf = this.panelConfigurations.get(id);
    return conf ? conf.position : undefined;
  }

  @action.bound
  setDirty(state: boolean): void {
    this.isDirty = state;
  }

  printMasterDetailTree() {
    const strrep = (cnt: number, str: string) => {
      let result = "";
      for (let i = 0; i < cnt; i++) result = result + str;
      return result;
    };

    const recursive = (dataView: IDataView, level: number) => {
      console.log(
        `${strrep(level, "  ")}${dataView.name} (${dataView.entity} - ${dataView.modelId})`
      );
      for (let chb of dataView.childBindings) {
        recursive(chb.childDataView, level + 1);
      }
    };
    console.log("");
    console.log("View bindings");
    console.log("=============");
    const roots = Array.from(this.dataViews.values()).filter((dv) => dv.isBindingRoot);
    for (let dv of roots) {
      recursive(dv, 0);
    }
    console.log("=============");
    console.log("End of View bindings");
    console.log("");
  }
}

export class FormScreenEnvelope implements IFormScreenEnvelope {
  $type_IFormScreenEnvelope: 1 = 1;

  constructor(data: IFormScreenEnvelopeData) {
    Object.assign(this, data);
    this.formScreenLifecycle.parent = this;
  }

  @observable formScreen?: IFormScreen | undefined;
  formScreenLifecycle: IFormScreenLifecycle02 = null as any;
  @computed get isLoading() {
    return !this.formScreen;
  }

  @action.bound
  setFormScreen(formScreen?: IFormScreen | undefined): void {
    if (formScreen) {
      formScreen.parent = this;
    }
    this.formScreen = formScreen;
  }

  *start(initUIResult: any, preloadIsDirty?: boolean): Generator {
    yield* this.formScreenLifecycle.start(initUIResult);
    if (this.formScreen) {
      this.formScreen.setDirty(!!preloadIsDirty);
    }
  }

  parent?: any;
}
