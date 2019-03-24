import { IUIScreenTreeNode } from "./IUIScreenBlueprints";
import { ITableView } from "./ITableViewPresenter/ITableView";
import { IFormView } from "./IFormViewPresenter/IFormView";

export interface IScreen {
  cardTitle: string;
  screenTitle: string;
  isLoading: boolean;
  isFullScreen: boolean;
  uiStructure: IUIScreenTreeNode[];
  dataViewsMap: Map<string, IDataView>;
  tabPanelsMap: Map<string, ITabs>; // Maps Id of Tab element to tab controller
  splittersMap: Map<string, ISplitter>; //
}

export enum IViewType {
  TableView = "TableView",
  FormView = "FormView"
}

export interface IViewTypeBtn {
  type: IViewType;
  btn: IToolbarButtonState;
}

export interface IToolbarButtonState {
  isEnabled: boolean;
  isVisible: boolean;
  isActive: boolean;
  onClick?(event: any): void;
}

export interface ITabPanel {
  label: string;
  id: string;
  content: React.ReactNode;
}

export interface ITabs {
  activeTabId: string;
  onHandleClick?(event: any, handleId: string): void;
}

export interface ISplitter {
  sizes: Map<string, number>;
}

export type ISpecificDataView = ITableView | IFormView;

export interface IDataView {
  id: string;
  activeView: ISpecificDataView | undefined;
  activeViewType: IViewType;
  availableViews: ISpecificDataView[];
  setActiveViewType(viewType: IViewType): void;
}
