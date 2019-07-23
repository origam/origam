import { IActionParameter } from "./IActionParameter";


export enum IActionType {
  Report = "Report",
  ChangeUI = "ChangeUI",
  OpenForm = "OpenForm",
  Workflow = "Workflow",
  QueueAction = "QueueAction",
  SelectionDialogAction = "SelectionDialogAction",
  Dropdown = "Dropdown"
}

export enum IActionPlacement {
  PanelHeader = "PanelHeader",
  Toolbar = "Toolbar"
}

export enum IActionMode {
  MultipleCheckboxes = "MultipleCheckboxes",
  ActiveRecord = "ActiveRecord",
  Always = "Always"
}

export interface IActionData {
  type: IActionType;
  id: string;
  groupId: string;
  caption: string;
  placement: IActionPlacement;
  iconUrl: string;
  mode: IActionMode;
  isDefault: boolean;
  parameters: IActionParameter[];
}

export interface IAction extends IActionData {
  $type_IAction: 1;

  parent?: any;
}
