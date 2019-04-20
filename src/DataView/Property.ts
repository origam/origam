import { observable } from "mobx";
import { IProperty } from "./types/IProperty";
import { parseBoolean } from "../utils/xml";
import { ILookupResolver } from "./Lookup/types/ILookupResolver";
import { IApi } from "../Api/IApi";
import { LookupResolver } from "./Lookup/LookupResolver";

export function buildProperty(
  xmlObj: any,
  dataIndex: number,
  menuItemId: string,
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
    dataIndex,
    lookupResolver
  );
}

export class Property implements IProperty {
  constructor(
    public id: string,
    public name: string,
    public isReadOnly: boolean,
    dataIndex: number,
    public lookupResolver: ILookupResolver | undefined
  ) {
    this.dataIndex = dataIndex;
  }
  @observable
  dataIndex: number;
}
