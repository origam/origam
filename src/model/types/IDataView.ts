import { IPanelViewType } from "./IPanelViewType";
import { IProperty } from "./IProperty";
import { IDataSource } from "./IDataSource";
import { IDataTable } from "./IDataTable";
import { IComponentBinding } from "./IComponentBinding";
import { IDataViewLifecycle } from "./IDataViewLifecycle";
import { ITablePanelView } from "../TablePanelView/types/ITablePanelView";
import { IFormPanelView } from '../FormPanelView/types/IFormPanelView';

export const CDataView = "CDataView";

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
  dataTable: IDataTable;
  formViewUI: any;
  activePanelView: IPanelViewType;
  tablePanelView: ITablePanelView;
  formPanelView: IFormPanelView;
  lifecycle: IDataViewLifecycle;
}

export interface IDataView extends IDataViewData {
  $type: typeof CDataView;
  isBindingRoot: boolean;
  parentBindings: IComponentBinding[];
  childBindings: IComponentBinding[];
  isWorking: boolean;

  selectedRowId: string | undefined;

  dataSource: IDataSource;
  

  onFormPanelViewButtonClick(event: any): void;
  onTablePanelViewButtonClick(event: any): void;

  run(): void;

  parent?: any;
}
