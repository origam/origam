import {IPluginTableRow} from "./IPluginRow";
import {IPluginProperty} from "./IPluginProperty";

export interface IPluginDataView {
  tableRows: IPluginTableRow[];
  properties: IPluginProperty[];
  getCellText(value: any, propertyId: string): any;
}