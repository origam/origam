/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o. 

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

import { OrigamDataType } from 'src/API/IArchitectApi.ts';

export enum ComponentType {
  AsCombo = 'Origam.Gui.Win.AsDropDown',
  AsTextBox = 'Origam.Gui.Win.AsTextBox',
  TagInput = 'Origam.Gui.Win.TagInput',
  AsPanel = 'Origam.Gui.Win.AsPanel',
  AsDateBox = 'Origam.Gui.Win.AsDateBox',
  AsCheckBox = 'Origam.Gui.Win.AsCheckBox',
  GroupBox = 'Origam.Gui.Win.GroupBoxWithChamfer',
  TextArea = 'TextArea',
  AsForm = 'Origam.Gui.Win.AsForm',
  FormPanel = 'Origam.Schema.GuiModel.PanelControlSet',
  SplitPanel = 'Origam.Gui.Win.SplitPanel',
  TabControl = 'Origam.Gui.Win.AsTabControl',
  TabPage = 'System.Windows.Forms.TabPage',
  AsTree = 'Origam.Gui.Win.AsTreeView',
}

export function parseComponentType(value: string): ComponentType {
  const validTypes = Object.values(ComponentType);

  if (!validTypes.includes(value as ComponentType)) {
    throw new Error(`Invalid component type: ${value}. Valid types are: ${validTypes.join(', ')}`);
  }

  return value as ComponentType;
}

export interface IComponentData {
  type: ComponentType;
  identifier?: string;
}

export function toComponentType(origamType: OrigamDataType): ComponentType {
  switch (origamType) {
    case OrigamDataType.Date:
      return ComponentType.AsDateBox;
    case OrigamDataType.String:
      return ComponentType.AsTextBox;
    case OrigamDataType.Memo:
      return ComponentType.TextArea;
    case OrigamDataType.Integer:
    case OrigamDataType.Float:
    case OrigamDataType.Long:
      return ComponentType.AsTextBox;
    case OrigamDataType.Boolean:
      return ComponentType.AsCheckBox;
    default:
      return ComponentType.AsTextBox;
  }
}

export function getComponentTypeKey(componentTypeValue: ComponentType): string | undefined {
  return Object.entries(ComponentType).find(entry => entry[1] === componentTypeValue)?.[0];
}
