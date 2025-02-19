/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

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
  Toolbar = "Toolbar",
  PanelMenu = "PanelMenu"
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
  confirmationMessage?: string;
  showAlways: boolean;
}

export interface IAction extends IActionData {
  $type_IAction: 1;
  isEnabled: boolean;
  parent?: any;
}

export const isIAction = (o: any): o is IAction => o.$type_IAction;