import { IDataView, IDataViewData, CDataView } from "./types/IDataView";
import { IPanelViewType } from "./types/IPanelViewType";
import { IProperty } from "./types/IProperty";
import { observable, action } from "mobx";

export class DataView implements IDataView {
  $type: typeof CDataView = CDataView;

  constructor(data: IDataViewData) {
    Object.assign(this, data);
    this.properties.forEach(o => (o.parent = this));
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
  formViewUI: any;
  @observable activePanelView: IPanelViewType = IPanelViewType.Table;

  @action.bound
  onFormPanelViewButtonClick(event: any) {
    this.activePanelView = IPanelViewType.Form;
  }

  @action.bound
  onTablePanelViewButtonClick(event: any) {
    this.activePanelView = IPanelViewType.Table;
  }
}
