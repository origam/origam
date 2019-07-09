import { IPanelViewType } from "./IPanelViewType";
import { IProperty } from "./IProperty";
import { IDataSource } from "./IDataSource";
import { IDataTable } from "./IDataTable";
import { IComponentBinding } from "./IComponentBinding";
import { IDataViewLifecycle } from "./IDataViewLifecycle";

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
  lifecycle: IDataViewLifecycle;
}

export interface IDataView extends IDataViewData {
  $type: typeof CDataView;
  tableViewProperties: IProperty[];
  isBindingRoot: boolean;
  parentBindings: IComponentBinding[];
  childBindings: IComponentBinding[];
  isWorking: boolean;

  dataSource: IDataSource;

  onFormPanelViewButtonClick(event: any): void;
  onTablePanelViewButtonClick(event: any): void;

  run(): void;

  parent?: any;
}
