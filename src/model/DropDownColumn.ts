import { IDropDownColumn, IDropDownColumnData } from "./types/IDropDownColumn";

export class DropDownColumn implements IDropDownColumn {
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
