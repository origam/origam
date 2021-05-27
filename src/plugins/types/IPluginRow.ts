export type IPluginTableRow = any[] | IPluginGroupRow;

export interface IPluginGroupRow {
  groupLevel: number;
  columnLabel: string;
  columnValue: string;
  isExpanded: boolean;
}
