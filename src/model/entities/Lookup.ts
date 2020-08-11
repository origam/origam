import { IDropDownColumn } from "./types/IDropDownColumn";
import { IDropDownParameter, IDropDownType, ILookup, ILookupData } from "./types/ILookup";

export enum IIdState {
  LOADING = "LOADING",
  ERROR = "ERROR",
}

export class Lookup implements ILookup {
  constructor(data: ILookupData) {
    Object.assign(this, data);
    this.dropDownColumns.forEach((o) => (o.parent = this));
  }
  $type_ILookup: 1 = 1;

  lookupId: string = "";
  dropDownShowUniqueValues: boolean = false;
  identifier: string = "";
  identifierIndex: number = 0;
  dropDownType: IDropDownType = IDropDownType.EagerlyLoadedGrid;
  cached: boolean = false;
  searchByFirstColumnOnly: boolean = false;
  dropDownColumns: IDropDownColumn[] = [];
  dropDownParameters: IDropDownParameter[] = [];

  parent?: any;

  get parameters() {
    const parameters: { [key: string]: any } = {};
    for (let param of this.dropDownParameters) {
      parameters[param.parameterName] = param.fieldName;
    }
    return parameters;
  }
}
