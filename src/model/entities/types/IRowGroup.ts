export interface IRowGroup {
  isExpanded: boolean;
  level: number;
  groupColumnName: string;
  groupValue: any;
  groupCaption: string;
  rowCount: number;
  groupChildren: IRowGroup[];
  rowChildren: any[][];
  parent: IRowGroup | undefined;
}