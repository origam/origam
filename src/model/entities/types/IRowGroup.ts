export interface IRowGroup {
  isExpanded: boolean;
  level: number;
  groupColumnName: string;
  groupValue: any;
  rowCount: number;
  groupChildren: IRowGroup[];
  rowChildren: any[][];
}