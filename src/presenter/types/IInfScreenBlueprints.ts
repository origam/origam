import { IUIScreenTreeNode, IUIFormRoot } from "./IUIScreenBlueprints";
import { ICaptionPosition } from "./IFormViewPresenter/ICaptionPosition";
import { ITabs, IViewType } from "./IScreenPresenter";

export interface IInfScreenBlueprints {

}

export interface IGridProperty {
  allowReturnToForm: boolean;
  autoSort: boolean;
  cached: boolean;
  captionLength: number | undefined;
  captionPosition: ICaptionPosition;
  column: string;
  entity: string;
  entityName: string | undefined;
  height: number | undefined;
  id: string;
  identifier: string | undefined;
  identifierIndex: number | undefined;
  isTree: boolean;
  lookupId: string | undefined;
  modelInstanceId: string | undefined;
  name: string;
  readOnly: boolean;
  searchByFirstColumnOnly: boolean;
  width: number | undefined;
  top: number | undefined;
  left: number | undefined;
}

export interface IDropDownProperty {
  column: string;
  entity: string;
  id: string;
  name: string;
  index: number;
}


export interface IDataView {
  id: string;
  name: string;
  isHeadless: boolean;
  initialView: IViewType;
  availableViews: Array<IFormView | ITableView>,
  properties: IGridProperty[],
  propertiesMap: Map<string, IGridProperty>
}

export interface IFormView {
  type: "FormView",
  uiStructure: IUIFormRoot[],
  isHeadless: boolean,
}

export interface ITableView {
  type: "TableView",
  properties: IGridProperty[],
}

export interface ITabbedPanel {
  activeTabId: string;
}

export interface IScreen {
  cardTitle: string;
  screenTitle: string;
  uiStructure: IUIScreenTreeNode[];
  dataViewsMap: Map<string, IDataView>;
  tabPanelsMap: Map<string, ITabbedPanel>;
}