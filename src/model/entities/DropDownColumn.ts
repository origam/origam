import { IDropDownColumn, IDropDownColumnData } from "./types/IDropDownColumn";

export class DropDownColumn implements IDropDownColumn {
  $type_IDropDownColumn: 1 = 1;
  
  constructor(data: IDropDownColumnData) {
    Object.assign(this, data);
  }
  
  parent: any;

  id: string = "";
  name: string = "";
  entity: string = "";
  column: string = "";
  index: number = 0;
}
