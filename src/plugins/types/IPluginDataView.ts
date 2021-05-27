import {IPluginTableRow} from "./IPluginRow";
import {IPluginProperty} from "./IPluginProperty";

export interface IPluginDataView {
  tableRows: IPluginTableRow[];
  properties: IPluginProperty[];
  getValue(row: any[], propertyId: string): any;
}