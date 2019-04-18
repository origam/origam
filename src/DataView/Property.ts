import { observable } from "mobx";
import { IProperty } from "./types/IProperty";
import { parseBoolean } from "../utils/xml";

export function buildProperty(xmlObj: any, dataIndex: number) {
  return new Property(
    xmlObj.attributes.Id,
    xmlObj.attributes.Name,
    parseBoolean(xmlObj.attributes.ReadOnly),
    dataIndex
  );
}

export class Property implements IProperty {
  constructor(
    public id: string,
    public name: string,
    public isReadOnly: boolean,
    dataIndex: number
  ) {
    this.dataIndex = dataIndex;
  }
  @observable
  dataIndex: number;
}
