import { IPropertyId } from "../../values/types/IPropertyId";
import { IProperty } from "./IProperty";

export interface IProperties {
  count: number;
  byId(id: IPropertyId): IProperty | undefined;
  byIndex(idx: number): IProperty | undefined;
  getIndex(property: IProperty): number | undefined;
  index2Id(idx: number): IPropertyId | undefined;
  id2Index(id: IPropertyId): number | undefined;
  getPropertyIdAfterId(id: IPropertyId): IPropertyId | undefined;
  getPropertyIdBeforeId(id: IPropertyId): IPropertyId | undefined;
}