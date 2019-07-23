import { IPanelViewType } from "./IPanelViewType";
import { IProperty } from "./IProperty";
import { IDataSource } from "./IDataSource";
import { IDataTable } from "./IDataTable";
import { IComponentBinding } from "./IComponentBinding";
import { IDataViewLifecycle } from "./IDataViewLifecycle";
import { ITablePanelView } from "../TablePanelView/types/ITablePanelView";
import { IFormPanelView } from "../FormPanelView/types/IFormPanelView";
import { IAction } from "./IAction";

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
}

export interface IDataView extends IDataViewData {
  $type_IDataView: 1;

  isBindingRoot: boolean;
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
  selectedRow: any[] | undefined;
  dataSource: IDataSource;

  onFormPanelViewButtonClick(event: any): void;
  onTablePanelViewButtonClick(event: any): void;
  onNextRowClick(event: any): void;
  onPrevRowClick(event: any): void;

  onFieldChange(event: any, row: any[], property: IProperty, value: any): void;
  selectFirstRow(): void;
  selectRowById(id: string | undefined): void;
  selectRow(row: any[]): void;
  setSelectedRowId(id: string | undefined): void;
  setEditing(state: boolean): void;

  run(): void;

  parent?: any;
}

export const isIDataView = (o: any): o is IDataView => o.$type_IDataView;
