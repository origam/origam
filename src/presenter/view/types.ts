export interface IUIScreen {} // ? Keep or kick out ?

export interface IUITabbedPanel {
  type: "Tab";
  props: {
    Id: string;
  };
  children: [];
}

export interface IUIVSplit {
  type: "VSplit";
  props: {};
  children: IUIScreenTreeNode[];
}

export interface IUIHSplit {
  type: "HSplit";
  props: {};
  children: IUIScreenTreeNode[];
}

export interface IUIVBox {
  type: "VBox";
  props: { Height: string | undefined };
  children: IUIScreenTreeNode[];
}

export interface IUIBox {
  type: "Box";
  props: { Id: string };
  children: IUIScreenTreeNode[];
}

export interface IUILabel {
  type: "Label";
  props: { Text: string; Height: string };
  children: IUIScreenTreeNode[];
}

export interface IUIDataView {
  type: "Grid";
  props: { Id: string };
  children: IUIScreenTreeNode[];
}

export interface IUITreePanel {
  type: "TreePanel";
  props: {};
  children: IUIScreenTreeNode[];
}

export type IUIScreenTreeNode =
  | IUITabbedPanel
  | IUIVSplit
  | IUIHSplit
  | IUIVBox
  | IUIBox
  | IUILabel
  | IUIDataView
  | IUITreePanel;

export interface ITabs {
  activeTabId: string;
  onHandleClick?(event: any, handleId: string): void;
}

export interface ISplitter {
  sizes: Map<string, number>;
}

export interface IScreen {
  cardTitle: string;
  screenTitle: string;
  isLoading: boolean;
  uiStructure: IUIScreenTreeNode[];
  dataViewsMap: Map<string, any>;
}
