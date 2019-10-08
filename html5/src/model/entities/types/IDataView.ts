import { IPanelViewType } from "./IPanelViewType";
import { IProperty } from "./IProperty";
import { IDataSource } from "./IDataSource";
import { IDataTable } from "./IDataTable";
import { IComponentBinding } from "./IComponentBinding";
import { IDataViewLifecycle } from "../DataViewLifecycle/types/IDataViewLifecycle";
import { ITablePanelView } from "../TablePanelView/types/ITablePanelView";
import { IFormPanelView } from "../FormPanelView/types/IFormPanelView";
import { IAction } from "./IAction";
import { IRowState } from "./IRowState";
import { ILookupLoader } from "./ILookupLoader";

export interface IDataViewData {
  id: string;
  modelInstanceId: string;
  name: string;
  modelId: string;
  defaultPanelView: IPanelViewType;
  isHeadless: boolean;
  disableActionButtons: boolean;
  showAddButton: boolean;
  showDeleteButton: boolean;
  showSelectionCheckboxes: boolean;
  isGridHeightDynamic: boolean;
  selectionMember: string;
  orderMember: string;
  isDraggingEnabled: boolean;
  entity: string;
  dataMember: string;
  isRootGrid: boolean;
  isRootEntity: boolean;
  isPreloaded: boolean;
  requestDataAfterSelectionChange: boolean;
  confirmSelectionChange: boolean;
  properties: IProperty[];
  actions: IAction[];
  dataTable: IDataTable;
  formViewUI: any;
  activePanelView: IPanelViewType;
  tablePanelView: ITablePanelView;
  formPanelView: IFormPanelView;
  lifecycle: IDataViewLifecycle;
  lookupLoader: ILookupLoader;
}

export interface IDataView extends IDataViewData {
  $type_IDataView: 1;

  isBindingRoot: boolean;
  isBindingParent: boolean;
  isAnyBindingAncestorWorking: boolean;
  parentBindings: IComponentBinding[];
  childBindings: IComponentBinding[];
  bindingRoot: IDataView;
  bindingParent: IDataView | undefined;
  isWorking: boolean;
  isEditing: boolean;
  isValidRowSelection: boolean;
  selectedRowId: string | undefined;
  selectedRowIndex: number | undefined;
  visibleRowCount: number;
  selectedRow: any[] | undefined;
  dataSource: IDataSource;
  bindingParametersFromParent: {[key: string]: string};

  isReorderedOnClient: boolean;

  panelViewActions: IAction[];
  toolbarActions: IAction[];
  dialogActions: IAction[];

  onFormPanelViewButtonClick(event: any): void;
  onTablePanelViewButtonClick(event: any): void;
  
  selectNextRow(): void;
  selectPrevRow(): void;

  onFieldChange(event: any, row: any[], property: IProperty, value: any): void;
  selectFirstRow(): void;
  selectRowById(id: string | undefined): void;
  selectRow(row: any[]): void;
  setSelectedRowId(id: string | undefined): void;
  setEditing(state: boolean): void;

  start(): void;

  parent?: any;
}

export const isIDataView = (o: any): o is IDataView => o.$type_IDataView;
