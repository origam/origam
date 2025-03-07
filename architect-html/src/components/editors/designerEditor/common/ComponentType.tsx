import { OrigamDataType } from "src/API/IArchitectApi.ts";

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
  AsTree= 'Origam.Gui.Win.AsTreeView'
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
  return Object.entries(ComponentType)
    .find(entry => entry[1] === componentTypeValue)?.[0];
}
