import { IDataView, IDataViewData } from "./types/IDataView";
import { IPanelViewType } from "./types/IPanelViewType";
import { IProperty } from "./types/IProperty";
import { observable, action, computed, runInAction } from "mobx";
import { IDataTable } from "./types/IDataTable";
import { getFormScreen } from "../selectors/FormScreen/getFormScreen";
import { IDataViewLifecycle } from "./types/IDataViewLifecycle";
import { getDataSourceByEntity } from "../selectors/DataSources/getDataSourceByEntity";
import { ITablePanelView } from "./TablePanelView/types/ITablePanelView";
import { IFormPanelView } from "./FormPanelView/types/IFormPanelView";
import { getDataTable } from "../selectors/DataView/getDataTable";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { IAction, IActionPlacement, IActionType } from "./types/IAction";


export class DataView implements IDataView {
  $type_IDataView: 1 = 1;

  constructor(data: IDataViewData) {
    Object.assign(this, data);
    this.properties.forEach(o => (o.parent = this));
    this.actions.forEach(o => (o.parent = this));
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
  actions: IAction[] = [];

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

  @computed get panelViewActions() {
    return this.actions.filter(
      action => action.placement === IActionPlacement.PanelHeader
    );
  }

  @computed get toolbarActions() {
    return this.actions.filter(
      action =>
        action.placement === IActionPlacement.Toolbar &&
        action.type !== IActionType.SelectionDialogAction
    );
  }

  @computed get dialogActions() {
    return this.actions.filter(
      action => action.type === IActionType.SelectionDialogAction
    )
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

  @action.bound
  onNextRowClick(event: any): void {
    const selectedRowId = getSelectedRowId(this);
    const newId = selectedRowId
      ? getDataTable(this).getNextExistingRowId(selectedRowId)
      : undefined;
    if (newId) {
      this.selectRowById(newId);
    }
  }

  @action.bound
  onPrevRowClick(event: any): void {
    const selectedRowId = getSelectedRowId(this);
    const newId = selectedRowId
      ? getDataTable(this).getPrevExistingRowId(selectedRowId)
      : undefined;
    if (newId) {
      this.selectRowById(newId);
    }
  }

  @action.bound onFieldChange(
    event: any,
    row: any[],
    property: IProperty,
    value: any
  ) {
    if (!property.readOnly) {
      getDataTable(this).setFormDirtyValue(row, property.id, value);
    }
  }

  @action.bound selectFirstRow() {
    const dataTable = getDataTable(this);
    const firstRow = dataTable.getFirstRow();
    if (firstRow) {
      this.selectRowById(dataTable.getRowId(firstRow));
    }
  }

  @action.bound selectRowById(id: string | undefined) {
    if (id !== this.selectedRowId) {
      //cannotChangeRowDialog(this);
      //return
      this.setSelectedRowId(id);
    }
  }

  @action.bound selectRow(row: any[]) {
    this.selectRowById(this.dataTable.getRowId(row));
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
