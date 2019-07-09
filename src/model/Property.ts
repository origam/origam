import { IPropertyData, IProperty } from "./types/IProperty";
import { ICaptionPosition } from "./types/ICaptionPosition";
import { IDropDownColumn } from "./types/IDropDownColumn";
import { IPropertyColumn } from "./types/IPropertyColumn";

export class Property implements IProperty {
  constructor(data: IPropertyData) {
    Object.assign(this, data);
    this.dropDownColumns.forEach(o => (o.parent = this));
  }

  parent: any;

  id: string = "";
  modelInstanceId: string = "";
  name: string = "";
  readOnly: boolean = false;
  x: number = 0;
  y: number = 0;
  width: number = 0;
  height: number = 0;
  captionLength: number = 0;
  captionPosition?: ICaptionPosition;
  entity: string = "";
  column: IPropertyColumn = IPropertyColumn.Text;
  dock?: string | undefined;
  multiline: boolean = false;
  isPassword: boolean = false;
  isRichText: boolean = false;
  maxLength: number = 0;
  dropDownShowUniqueValues?: boolean | undefined;
  lookupId?: string | undefined;
  identifier?: string | undefined;
  identifierIndex?: number | undefined;
  dropDownType?: string | undefined;
  cached?: boolean | undefined;
  searchByFirstColumnOnly?: boolean | undefined;
  allowReturnToForm?: boolean | undefined;
  isTree?: boolean | undefined;
  dropDownColumns: IDropDownColumn[] = [];
  lookupCache: Map<string, any> = new Map();
  dataIndex: number = 0;
}
