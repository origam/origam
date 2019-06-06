import { observable } from "mobx";
import { IProperty } from "./types/IProperty";
import { parseBoolean } from "../utils/xml";
import { ILookupResolver } from "./Lookup/types/ILookupResolver";
import { IApi } from "../Api/IApi";
import { LookupResolver } from "./Lookup/LookupResolver";

export function buildProperty(
  xmlObj: any,
  dataIndex: number,
  dataSourceIndex: number,
  menuItemId: string,
  lookupColumns: string[],
  api: IApi
) {
  let lookupResolver: ILookupResolver | undefined;
  if (xmlObj.attributes.LookupId) {
    lookupResolver = new LookupResolver({
      lookupId: xmlObj.attributes.LookupId,
      menuItemId,
      api
    });
  }
  return new Property(
    xmlObj.attributes.Id,
    xmlObj.attributes.Name,
    parseBoolean(xmlObj.attributes.ReadOnly),
    xmlObj.attributes.Entity,
    xmlObj.attributes.Column,
    dataIndex,
    dataSourceIndex,
    lookupResolver,
    lookupColumns
  );
}

export class Property implements IProperty {
  constructor(
    public id: string,
    public name: string,
    public isReadOnly: boolean,
    public entity: string,
    public  column: string,
    dataIndex: number,
    public dataSourceIndex: number,
    public lookupResolver: ILookupResolver | undefined,
    public lookupColumns: string[]
  ) {
    this.dataIndex = dataIndex;
  }
  @observable
  dataIndex: number;
}
