export type IPluginTableRow = IPluginRow | IPluginGroupRow;

export interface IPluginRow {
  [key: string]: string;
}

export interface IPluginGroupRow {
  groupLevel: number;
  columnLabel: string;
  columnValue: string;
  isExpanded: boolean;
}
