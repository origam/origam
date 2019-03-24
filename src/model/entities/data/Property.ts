import { IProperty } from "./types/IProperty";
import { ILookup } from "./types/ILookup";
import { IRecord } from "./types/IRecord";

interface IPropertyParam {
  id: string;  
  name: string;
  entity: string;
  column: string;
  isReadOnly: boolean;
  recordDataIndex: number;
  lookup: ILookup | undefined;
}

export class Property implements IProperty {
  
  constructor(param: IPropertyParam) {
    this.id = param.id;
    this.name = param.name;
    this.entity = param.entity;
    this.column = param.column;
    this.isReadOnly = param.isReadOnly;
    this.recordDataIndex = param.recordDataIndex;
    this.lookup = param.lookup;
  }
  
  recordDataIndex: number;
  id: string;  
  name: string;
  entity: string;
  column: string;
  isReadOnly: boolean;
  lookup: ILookup | undefined;


}