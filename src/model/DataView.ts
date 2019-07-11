import { IDataView, IDataViewData, CDataView } from "./types/IDataView";
import { IPanelViewType } from "./types/IPanelViewType";
import { IProperty } from "./types/IProperty";
import { observable, action, computed, runInAction } from "mobx";
import { IDataTable } from "./types/IDataTable";
import { getFormScreen } from "./selectors/FormScreen/getFormScreen";
import { IDataViewLifecycle } from "./types/IDataViewLifecycle";
import { getDataSourceByEntity } from "./selectors/DataSources/getDataSourceByEntity";
import { ITablePanelView } from "./TablePanelView/types/ITablePanelView";
import { IFormPanelView } from "./FormPanelView/types/IFormPanelView";
import { getDataTable } from "./selectors/DataView/getDataTable";

export class DataView implements IDataView {

  $type: typeof CDataView = CDataView;

  constructor(data: IDataViewData) {
    Object.assign(this, data);
    this.properties.forEach(o => (o.parent = this));
    this.dataTable.parent = this;
    this.lifecycle.parent = this;
    this.tablePanelView.parent = this;
    this.formPanelView.parent = this;
    // Identifier - usualy Id is always the first property.
  }

  id = "";
  modelInstanceId = "";
  name = "";
  modelId = "";
  defaultPanelView = IPanelViewType.Table;
  isHeadless = false;
  disableActionButtons = false;
  showAddButton = false;
  showDeleteButton = false;
  showSelectionCheckboxes = false;
  isGridHeightDynamic = false;
  selectionMember = "";
  orderMember = "";
  isDraggingEnabled = false;
  entity = "";
  dataMember = "";
  isRootGrid = false;
  isRootEntity = false;
  isPreloaded = false;
  requestDataAfterSelectionChange = false;
  confirmSelectionChange = false;
  properties: IProperty[] = [];
  @observable tableViewProperties: IProperty[] = [];
  dataTable: IDataTable = null as any;
  formViewUI: any;
  lifecycle: IDataViewLifecycle = null as any;
  tablePanelView: ITablePanelView = null as any;
  formPanelView: IFormPanelView = null as any;

  @observable activePanelView: IPanelViewType = IPanelViewType.Table;
  @observable isEditing: boolean = false;

  @observable selectedRowId: string | undefined;
  @computed get selectedRowIndex(): number | undefined {
    return this.selectedRowId
      ? this.dataTable.getExistingRowIdxById(this.selectedRowId)
      : undefined;
  }
  @computed get selectedRow(): any[] | undefined {
    return this.selectedRowIndex !== undefined
      ? this.dataTable.getRowByExistingIdx(this.selectedRowIndex)
      : undefined;
  }
  @computed get isValidRowSelection(): boolean {
    return this.selectedRowIndex !== undefined;
  }

  get isWorking() {
    return this.lifecycle.isWorking;
  }

  @computed get isAnyBindingAncestorWorking() {
    if (this.isBindingRoot) {
      return false;
    } else {
      return (
        this.bindingParent.isWorking ||
        this.bindingParent.isAnyBindingAncestorWorking
      );
    }
  }

  @computed
  get isBindingRoot() {
    return this.parentBindings.length === 0;
  }

  @computed get bindingParent() {
    return this.parentBindings[0].parentDataView;
  }

  @computed get bindingRoot(): IDataView {
    // TODO: If there ever is multiparent case, remove duplicates in the result
    let root: IDataView = this;
    while (!root.isBindingRoot) {
      root = root.bindingParent!;
    }
    return root;
  }

  @computed
  get parentBindings() {
    const screen = getFormScreen(this);
    return screen.getBindingsByChildId(this.modelInstanceId);
  }

  @computed
  get childBindings() {
    const screen = getFormScreen(this);
    return screen.getBindingsByParentId(this.modelInstanceId);
  }

  @computed get dataSource() {
    return getDataSourceByEntity(this, this.entity)!;
  }

  @action.bound
  onFormPanelViewButtonClick(event: any) {
    this.activePanelView = IPanelViewType.Form;
  }

  @action.bound
  onTablePanelViewButtonClick(event: any) {
    this.activePanelView = IPanelViewType.Table;
  }

  @action.bound onFieldChange(
    event: any,
    row: any[],
    property: IProperty,
    value: any
  ) {
    getDataTable(this).setFormDirtyValue(row, property.id, value);
  }

  @action.bound selectFirstRow() {
    const dataTable = getDataTable(this);
    const firstRow = dataTable.getFirstRow();
    if (firstRow) {
      this.selectRow(firstRow[0]);
    }
  }

  @action.bound selectRow(id: string | undefined) {
    this.setSelectedRowId(id);
  }

  @action.bound
  setSelectedRowId(id: string | undefined): void {
    this.selectedRowId = id;
  }

  setEditing(state: boolean): void {
    this.isEditing = state;
  }

  @action.bound run() {
    this.lifecycle.run();
  }

  parent?: any;
}
