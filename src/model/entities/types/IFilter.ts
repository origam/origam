import { IFilterSetting } from "./IFilterSetting";

export interface IFilter {
  propertyId: string;
  dataType: string;
  setting: IFilterSetting;
}