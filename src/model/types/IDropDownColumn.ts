export interface IDropDownColumnData {
  id: string;
  name: string;
  entity: string;
  column: string;
  index: number;
}

export interface IDropDownColumn extends IDropDownColumnData {
  parent?: any;
}
