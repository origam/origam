import { action, computed, observable, reaction } from "mobx";
import { getParentRow } from "model/selectors/DataView/getParentRow";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getDataSourceByEntity } from "../selectors/DataSources/getDataSourceByEntity";
import { getDataTable } from "../selectors/DataView/getDataTable";
import { getFormScreen } from "../selectors/FormScreen/getFormScreen";
import { getIsDialog } from "../selectors/getIsDialog";
import { IDataViewLifecycle } from "./DataViewLifecycle/types/IDataViewLifecycle";
import { IFormPanelView } from "./FormPanelView/types/IFormPanelView";
import { ITablePanelView } from "./TablePanelView/types/ITablePanelView";
import { IAction, IActionPlacement, IActionType } from "./types/IAction";
import { IDataTable } from "./types/IDataTable";
import { IDataView, IDataViewData } from "./types/IDataView";
import { IPanelViewType } from "./types/IPanelViewType";
import { IProperty } from "./types/IProperty";
import { getBindingToParent } from "model/selectors/DataView/getBindingToParent";
import { getDataSourceFieldByName } from "model/selectors/DataSources/getDataSourceFieldByName";
import { getEntity } from "model/selectors/DataView/getEntity";
import { getBindingParent } from "model/selectors/DataView/getBindingParent";
import { ILookupLoader } from "./types/ILookupLoader";
import bind from "bind-decorator";
import { getRowStateMayCauseFlicker } from "model/selectors/RowState/getRowStateMayCauseFlicker";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { ServerSideGrouper } from "./ServerSideGrouper";
import { ClientSideGrouper } from "./ClientSideGrouper";
import { getFormScreenLifecycle } from "../selectors/FormScreen/getFormScreenLifecycle";
import { getTableViewProperties } from "../selectors/TablePanelView/getTableViewProperties";
import { getIsSelectionCheckboxesShown } from "../selectors/DataView/getIsSelectionCheckboxesShown";
import { getGroupingConfiguration } from "../selectors/TablePanelView/getGroupingConfiguration";
import { flattenToTableRows } from "../../gui/Components/ScreenElements/Table/TableRendering/tableRows";
import { GridDimensions } from "../../gui/Workbench/ScreenArea/TableView/GridDimensions";
import { SimpleScrollState } from "../../gui/Components/ScreenElements/Table/SimpleScrollState";
import { BoundingRect } from "react-measure";
import { IGridDimensions } from "../../gui/Components/ScreenElements/Table/types";
import { FocusManager } from "./FocusManager";
import { getRowStates } from "model/selectors/RowState/getRowStates";
import { getLookupLoader } from "model/selectors/DataView/getLookupLoader";
import { DataViewData } from "../../modules/DataView/DataViewData";
import { DataViewAPI } from "../../modules/DataView/DataViewAPI";
import { RowCursor } from "../../modules/DataView/TableCursor";
import {getDontRequestData} from "model/selectors/getDontRequestData";
import {
  IInfiniteScrollLoader,
  InfiniteScrollLoader, NullIScrollLoader
} from "gui/Workbench/ScreenArea/TableView/InfiniteScrollLoader";
import { ScrollRowContainer } from "model/entities/ScrollRowContainer";
import { VisibleRowsMonitor } from "gui/Workbench/ScreenArea/TableView/VisibleRowsMonitor";
import { getSelectionMember } from "model/selectors/DataView/getSelectionMember";

class SavedViewState {
  constructor(public selectedRowId: string | undefined) {}
}

export class DataView implements IDataView {
  $type_IDataView: 1 = 1;
  focusManager: FocusManager = new FocusManager();

  constructor(data: IDataViewData) {
    Object.assign(this, data);
    //this.showSelectionCheckboxes = true;
    //this.showSelectionCheckboxes = false;
    this.properties.forEach((o) => (o.parent = this));
    this.actions.forEach((o) => (o.parent = this));
    this.defaultActions = this.actions.filter((action) => action.isDefault);
    this.dataTable.parent = this;
    this.lifecycle.parent = this;
    this.tablePanelView.parent = this;
    this.formPanelView.parent = this;
    this.lookupLoader.parent = this;
    this.clientSideGrouper.parent = this;
    this.serverSideGrouper.parent = this;

    this.gridDimensions = new GridDimensions({
      getTableViewProperties: () => getTableViewProperties(this),
      getRowCount: () => this.tableRows.length,
      getIsSelectionCheckboxes: () => getIsSelectionCheckboxesShown(this.tablePanelView),
      ctx: this,
      defaultRowHeight: this.tablePanelView.rowHeight,
    });

    this.orderProperty = this.properties.find(prop => prop.name === this.orderMember)!;
    this.dataTable.rowRemovedListeners.push(() => this.selectAllCheckboxChecked = false);
  }

  orderProperty: IProperty;
  activateFormView: (()=> Generator) | undefined;
  gridDimensions: IGridDimensions;

  isReorderedOnClient: boolean = true;

  id = "";
  modelInstanceId = "";
  name = "";
  modelId = "";
  defaultPanelView = IPanelViewType.Table;
  isHeadless = false;
  disableActionButtons = false;
  showAddButton = false;
  showDeleteButton = false;
  showSelectionCheckboxesSetting = false;
  isGridHeightDynamic = false;
  selectionMember = "";
  orderMember = "";
  isDraggingEnabled = false;
  entity = "";
  dataMember = "";
  isRootGrid = false;
  isRootEntity = false;
  isPreloaded = false;
  newRecordView: string | undefined;
  requestDataAfterSelectionChange = false;
  confirmSelectionChange = false;
  properties: IProperty[] = [];
  actions: IAction[] = [];
  defaultActions: IAction[] = [];
  type: string = "";

  @observable tableViewProperties: IProperty[] = [];
  dataTable: IDataTable = null as any;
  formViewUI: any;
  lifecycle: IDataViewLifecycle = null as any;
  tablePanelView: ITablePanelView = null as any;
  formPanelView: IFormPanelView = null as any;
  lookupLoader: ILookupLoader = null as any;
  serverSideGrouper: ServerSideGrouper = null as any;
  clientSideGrouper: ClientSideGrouper = null as any;
  isFirst: boolean = null as any;

  dataViewRowCursor: RowCursor = null as any;
  dataViewApi: DataViewAPI = null as any;
  dataViewData: DataViewData = null as any;

  @observable selectAllCheckboxChecked = false;
  @observable selectedRowIdsMap: Map<string, boolean> = new Map();

  @observable activePanelView: IPanelViewType = IPanelViewType.Table;
  @observable isEditing: boolean = false;

  @observable selectedRowId: string | undefined;

  @computed get showSelectionCheckboxes() {
    return this.showSelectionCheckboxesSetting || !!this.selectionMember;
  }

  @computed get firstEnabledDefaultAction(){
    return this.defaultActions.find(action  => action.isEnabled);
  }

  @bind hasSelectedRowId(id: string) {
    return this.selectedRowIdsMap.has(id);
  }

  @computed get isAnyRowIdSelected(): boolean {
    return this.selectedRowIdsMap.size > 0;
  }

  setRecords(rows: any[][]): void {
    this.dataTable.setRecords(rows);
    this.selectAllCheckboxChecked = this.dataTable.rows
      .every((row) => this.isSelected(this.dataTable.getRowId(row)));
  }


  @computed get selectedRowIds() {
    return Array.from(this.selectedRowIdsMap.keys());
  }

  isSelected(rowId: string): boolean {
    const selectionMember = getSelectionMember(this);
    if (!!selectionMember) {
      const dataSourceField = getDataSourceFieldByName(this, selectionMember)!;
      const updatedRow = this.dataTable.getRowById(rowId)!;
      return this.dataTable.getCellValueByDataSourceField(updatedRow, dataSourceField);
    }
    return !this.selectedRowIdsMap.has(rowId)
      ? false
      : this.selectedRowIdsMap.get(rowId)!;
  }

  @action.bound addSelectedRowId(id: string) {
    this.selectedRowIdsMap.set(id, true);
  }

  @action.bound removeSelectedRowId(id: string) {
    this.selectedRowIdsMap.delete(id);
  }

  @action.bound setSelectedState(rowId: string, newState: boolean){
    if (newState) {
      this.addSelectedRowId(rowId);
    } else {
      this.removeSelectedRowId(rowId);
    }
  }

  @computed get selectedRowIndex(): number | undefined {
    return this.selectedRowId
      ? this.dataTable.getExistingRowIdxById(this.selectedRowId)
      : undefined;
  }

  @computed get maxRowCountSeen() {
    return this.dataTable.maxRowCountSeen;
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
    const rowStateMayCauseFlicker = getRowStateMayCauseFlicker(this);
    if (rowStateMayCauseFlicker && !this.dataTable.isEmpty) {
      return [];
    }
    return this.actions.filter((action) => action.placement === IActionPlacement.PanelHeader);
  }

  @computed get toolbarActions() {
    const rowStateMayCauseFlicker = getRowStateMayCauseFlicker(this);
    if (rowStateMayCauseFlicker && !this.dataTable.isEmpty) {
      return [];
    }
    return this.actions.filter(
      (action) =>
        action.placement === IActionPlacement.Toolbar &&
        action.type !== IActionType.SelectionDialogAction &&
        !getIsDialog(this)
    );
  }

  @computed get dialogActions() {
    return this.actions.filter(
      (action) => action.type === IActionType.SelectionDialogAction || getIsDialog(this)
    );
  }

  @computed get isWorking() {
    // TODO
    return (
      this.lifecycle.isWorking || getRowStates(this).isWorking || getLookupLoader(this).isWorking
    );
  }

  @computed get isAnyBindingAncestorWorking() {
    if (this.isBindingRoot) {
      return false;
    } else {
      return this.bindingParent.isWorking || this.bindingParent.isAnyBindingAncestorWorking;
    }
  }

  @computed
  get isBindingRoot() {
    return this.parentBindings.length === 0;
  }

  @computed get isBindingParent() {
    return this.childBindings.length > 0;
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

  @computed get bindingParametersFromParent() {
    const parentRow = getParentRow(this);
    if (parentRow) {
      const parent = getBindingParent(this);
      const parentEntity = getEntity(parent);
      const parentDataTable = getDataTable(parent);

      const bindingToParent = getBindingToParent(this)!;
      const result: { [key: string]: string } = {};
      for (let bp of bindingToParent.bindingPairs) {
        const parentDataSourceField = getDataSourceFieldByName(parent, bp.parentPropertyId)!;
        result[bp.childPropertyId] = parentDataTable.getCellValueByDataSourceField(
          parentRow,
          parentDataSourceField
        );
      }
      return result;
    } else {
      return {};
    }
  }

  @action.bound moveSelectedRowUp(){
    if(!this.selectedRowId){
      return;
    }
    const dataTable = getDataTable(this);
    const selectedRow = dataTable.getRowById(this.selectedRowId)!;
    const positionIndex = this.orderProperty.dataIndex;
    const selectedRowPosition = selectedRow[positionIndex];
    const nextRow = dataTable.rows.find(row => row[positionIndex] ===  selectedRowPosition + 1);
    if(!nextRow){
      return;
    }
    selectedRow[positionIndex] += 1;
    nextRow[positionIndex] -= 1;
    this.dataTable.substituteRecord(selectedRow);
    this.dataTable.substituteRecord(nextRow);
    this.dataTable.updateSortAndFilter();
    this.dataTable.setDirtyValue(selectedRow, this.orderProperty.id, selectedRow[positionIndex]);
    this.dataTable.setDirtyValue(nextRow, this.orderProperty.id, nextRow[positionIndex]);
  }

  @action.bound moveSelectedRowDown(){
    if(!this.selectedRowId){
      return;
    }
    const dataTable = getDataTable(this);
    const selectedRow = dataTable.getRowById(this.selectedRowId)!;
    const positionIndex = this.orderProperty.dataIndex;
    const selectedRowPosition = selectedRow[positionIndex];
    const previous = dataTable.rows.find(row => row[positionIndex] ===  selectedRowPosition - 1);
    if(!previous){
      return;
    }
    selectedRow[positionIndex] -= 1;
    previous[positionIndex] += 1;
    this.dataTable.substituteRecord(selectedRow);
    this.dataTable.substituteRecord(previous);
    this.dataTable.updateSortAndFilter();
    this.dataTable.setDirtyValue(selectedRow, this.orderProperty.id, selectedRow[positionIndex]);
    this.dataTable.setDirtyValue(previous, this.orderProperty.id, previous[positionIndex]);
  }

  @action.bound selectNextRow() {
    const selectedRowId = getSelectedRowId(this);
    const newId = selectedRowId
      ? getDataTable(this).getNextExistingRowId(selectedRowId)
      : undefined;
    if (newId) {
      this.selectRowById(newId);
    }
  }

  @action.bound selectPrevRow() {
    const selectedRowId = getSelectedRowId(this);
    const newId = selectedRowId
      ? getDataTable(this).getPrevExistingRowId(selectedRowId)
      : undefined;
    if (newId) {
      this.selectRowById(newId);
    }
  }

  @action.bound onFieldChange(event: any, row: any[], property: IProperty, value: any) {
    if (!property.readOnly) {
      getDataTable(this).setFormDirtyValue(row, property.id, value);
    }
  }

  @action.bound selectFirstRow() {
    const dataTable = getDataTable(this);
    const firstRow = dataTable.getFirstRow();
    if (firstRow) {
      this.selectRowById(dataTable.getRowId(firstRow));
    } else {
      this.selectRowById(undefined);
    }
  }

  @action.bound selectLastRow() {
    const dataTable = getDataTable(this);
    const lastRow = dataTable.getLastRow();
    if (lastRow) {
      this.selectRowById(dataTable.getRowId(lastRow));
    } else {
      this.selectRowById(undefined);
    }
  }

  reselectOrSelectFirst() {
    const previouslySelectedRowExists =
      this.selectedRowId && this.dataTable.getRowById(this.selectedRowId);
    if (!this.isRootGrid || !previouslySelectedRowExists || this.selectedRow) {
      this.selectFirstRow();
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
    if(this.dataTable.addedRowPositionLocked){
      return;
    }
    this.selectedRowId = id;
    if(this.isBindingParent){
      this.childBindings.forEach(binding => binding.childDataView.dataTable.updateSortAndFilter());
    }
  }

  @action.bound
  setEditing(state: boolean): void {
    this.isEditing = state;
  }

  viewStateStack: SavedViewState[] = [];

  @action.bound
  saveViewState(): void {
    this.viewStateStack.push(new SavedViewState(this.selectedRowId));
  }

  @action.bound
  restoreViewState(): void {
    const state = this.viewStateStack.pop();
    if (state && state.selectedRowId) {
      this.setSelectedRowId(state.selectedRowId);
      if (!getSelectedRow(this)) {
        this.selectFirstRow();
      }
      getTablePanelView(this).scrollToCurrentCell();
    }
  }

  @action.bound
  async start() {
    this.lifecycle.start();
    const serverSideGrouping = getDontRequestData(this)
    if(serverSideGrouping){
      this.serverSideGrouper.start();
    }
    getFormScreenLifecycle(this).registerDisposer(() => this.serverSideGrouper.dispose());
    await this.dataTable.start();
    getFormScreenLifecycle(this).registerDisposer(
      reaction(
        () => ({
          selectedRow: this.selectedRow,
          rowsCount: getDataTable(this).allRows.length,
        }),
        (reData: { selectedRow: any[] | undefined; rowsCount: number }) => {
          if (reData.selectedRow === undefined && reData.rowsCount > 0) {
            this.reselectOrSelectFirst();
          }
        },
        {
          fireImmediately: true,
        }
      )
    );
    await this.dataTable.start();
  }

  @action.bound stop() {
    this.properties.forEach((prop) => prop.stop());
    this.dataTable.stop();
  }

  @computed get tableRows() {
    const groupedColumnIds = getGroupingConfiguration(this).orderedGroupingColumnIds;
    return groupedColumnIds.length === 0
      ? getDataTable(this).rows
      : flattenToTableRows(getDataTable(this).groups);
  }

  scrollState = new SimpleScrollState(0, 0);

  @observable contentBounds: BoundingRect | undefined;
  infiniteScrollLoader: IInfiniteScrollLoader | undefined;

  parent?: any;

  initializeNewScrollLoader() {
    if (this.infiniteScrollLoader) {
      this.infiniteScrollLoader.dispose();
    }
    this.infiniteScrollLoader = this.getScrollLoader();
    getFormScreenLifecycle(this).registerDisposer(this.infiniteScrollLoader.start());
  }

  getScrollLoader() {
    const isGroupingOff =
      getGroupingConfiguration(this).orderedGroupingColumnIds.length === 0;
    const rowsContainer = getDataTable(this).rowsContainer;
    if (rowsContainer instanceof ScrollRowContainer && isGroupingOff) {
      return new InfiniteScrollLoader({
        ctx: this,
        gridDimensions: this.gridDimensions,
        scrollState: this.scrollState,
        rowsContainer: rowsContainer as ScrollRowContainer,
        groupFilter: undefined,
        visibleRowsMonitor: new VisibleRowsMonitor(
          this,
          this.gridDimensions,
          this.scrollState
        ),
      });
    } else {
      return new NullIScrollLoader();
    }
  }

  // Called by client scripts
  focusFormViewControl(name: string) {
    this.focusManager.focus(name);
  }

  // Called by client scripts
  showView(viewId: string, focus: boolean) {
    throw new Error("showView method is not yet implemented.");
  }

  // Called by client scripts
  switchToPanel(modelInstanceId: string) {
    throw new Error("switchToPanel method is not yet implemented.");
  }

  onReload() {
    this.dataTable.unlockAddedRowPosition();
  }

  attributes: any;
}
