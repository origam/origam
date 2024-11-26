import { OrigamDataType } from "src/API/IArchitectApi.ts";

export enum ComponentType {
  AsCombo = 'AsCombo',
  AsTextBox = 'AsTextBox',
  TagInput = 'TagInput',
  AsPanel = 'AsPanel',
  AsDateBox = 'AsDateBox',
  AsCheckBox = 'AsCheckBox',
  GroupBox = 'GroupBox',
  TextArea = 'TextArea',
  TextEditor = 'TextEditor',
  NumericEditor = 'NumericEditor',
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
  name: string;
}

export function toComponentType(origamType: OrigamDataType): ComponentType {
  switch (origamType) {
    case OrigamDataType.Date:
      return ComponentType.AsDateBox;
    case OrigamDataType.String:
      return ComponentType.TextEditor;
    case OrigamDataType.Memo:
      return ComponentType.TextArea;
    case OrigamDataType.Integer:
    case OrigamDataType.Float:
    case OrigamDataType.Long:
      return ComponentType.NumericEditor;
    case OrigamDataType.Boolean:
      return ComponentType.AsCheckBox;
    default:
      return ComponentType.TextEditor;
  }
}
