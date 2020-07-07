import {IPanelViewType} from "./IPanelViewType";
import {IProperty} from "./IProperty";
import {IDataSource} from "./IDataSource";
import {IDataTable} from "./IDataTable";
import {IComponentBinding} from "./IComponentBinding";
import {IDataViewLifecycle} from "../DataViewLifecycle/types/IDataViewLifecycle";
import {ITablePanelView} from "../TablePanelView/types/ITablePanelView";
import {IFormPanelView} from "../FormPanelView/types/IFormPanelView";
import {IAction} from "./IAction";
import {ILookupLoader} from "./ILookupLoader";
import {ServerSideGrouper} from "../ServerSideGrouper";
import {ClientSideGrouper} from "../ClientSideGrouper";
import {IGridDimensions, IScrollState} from "../../../gui/Components/ScreenElements/Table/types";
import {ITableRow} from "../../../gui/Components/ScreenElements/Table/TableRendering/types";
import {BoundingRect} from "react-measure";
import {FocusManager} from "../FocusManager";

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
  showSelectionCheckboxesSetting: boolean;
  
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
  serverSideGrouper: ServerSideGrouper; 
  clientSideGrouper: ClientSideGrouper;
}

export interface IDataView extends IDataViewData {
 
  $type_IDataView: 1;

  isBindingRoot: boolean;
  isBindingParent: boolean;
  isAnyBindingAncestorWorking: boolean;
  isWorking: boolean;
  parentBindings: IComponentBinding[];
  childBindings: IComponentBinding[];
  bindingRoot: IDataView;
  bindingParent: IDataView | undefined;
  isEditing: boolean;
  isValidRowSelection: boolean;
  selectedRowId: string | undefined;
  selectedRowIndex: number | undefined;
  maxRowCountSeen: number;
  selectedRow: any[] | undefined;
  dataSource: IDataSource;
  bindingParametersFromParent: {[key: string]: string};
  showSelectionCheckboxes: boolean;
  isReorderedOnClient: boolean;
  selectedRowIdsMap: Map<string, boolean>;
  panelViewActions: IAction[];
  toolbarActions: IAction[];
  dialogActions: IAction[];
  focusManager: FocusManager;
  
  isSelected(id: string): boolean;
  hasSelectedRowId(id: string): boolean;
  selectedRowIds: string[];
  isAnyRowIdSelected: boolean;
  addSelectedRowId(id: string): void;
  removeSelectedRowId(id: string): void;
  toggleSelectedRowId(id: string): void;



  
  selectNextRow(): void;
  selectPrevRow(): void;

  onFieldChange(event: any, row: any[], property: IProperty, value: any): void;
  selectFirstRow(): void;
  selectRowById(id: string | undefined): void;
  selectRow(row: any[]): void;
  setSelectedRowId(id: string | undefined): void;
  setEditing(state: boolean): void;

  saveViewState(): void;
  restoreViewState(): void;

  start(): void;

  scrollState: IScrollState;
  tableRows: ITableRow[];
  gridDimensions: IGridDimensions;
  contentBounds: BoundingRect | undefined;

  parent?: any;
}

export const isIDataView = (o: any): o is IDataView => o.$type_IDataView;
