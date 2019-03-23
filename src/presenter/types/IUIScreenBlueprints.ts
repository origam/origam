export interface IPanelDef {
  label: string;
  id: string;
  content: IUIScreenTreeNode[];
}

export interface IUITabbedPanel {
  type: "TabbedPanel";
  props: {
    id: string;
    panels: IPanelDef[];
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
  props: { height: number | undefined };
  children: IUIScreenTreeNode[];
}

export interface IUIBox {
  type: "Box";
  props: {};
  children: IUIScreenTreeNode[];
}

export interface IUIRoot {
  type: "Root";
  props: {};
  children: IUIScreenTreeNode[];
}

export interface IUILabel {
  type: "Label";
  props: { text: string; height: number | undefined };
  children: IUIScreenTreeNode[];
}

export interface IUIDataView {
  type: "DataView";
  props: { id: string; height: number | undefined };
  children: IUIScreenTreeNode[];
}

export interface IUITreePanel {
  type: "TreePanel";
  props: {};
  children: IUIScreenTreeNode[];
}

export interface IUIFormRoot {
  type: "FormRoot";
  props: {};
  children: Array<IUIFormField | IUIFormSection>;
}

export interface IUIFormSection {
  type: "FormSection";
  props: {
    width: number;
    height: number;
    top: number;
    left: number;
    title: string;
  };
  children: IUIFormField[];
}

export interface IUIFormField {
  type: "FormField";
  props: {
    id: string;
    name?: string;
    captionLength?: number;
    captionPosition?: "Left" | "Top" | "Right";
    column?: string;
    entity?: string;
    height?: number;
    width?: number;
    top?: number;
    left?: number;
  };
  children: [];
}

export type IUIScreenTreeNode =
  | IUITabbedPanel
  | IUIVSplit
  | IUIHSplit
  | IUIVBox
  | IUIBox
  | IUILabel
  | IUIDataView
  | IUITreePanel
  | IUIRoot
  | IUIFormRoot;
