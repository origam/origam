import {IPluginData} from "../types/IPluginData";
import {IDataView} from "../../model/entities/types/IDataView";
import { IPluginTableRow} from "../types/IPluginRow";
import {getProperties} from "../../model/selectors/DataView/getProperties";
import {IPluginDataView} from "../types/IPluginDataView";
import {IPluginProperty} from "../types/IPluginProperty";


export function createPluginData(dataView: IDataView): IPluginData | undefined {
  if(!dataView){
    return undefined;
  }
  return {
    dataView: new PluginDataView(dataView)
  }
}

class PluginDataView implements IPluginDataView{
  properties: IPluginProperty[];

  get tableRows(): IPluginTableRow[]{
    return this.dataView.tableRows;
  }

  constructor(
    private dataView: IDataView,
  ){
    this.properties = getProperties(this.dataView);
  }

  getCellText(row: any[], propertyId: string): any {
    const property = getProperties(this.dataView).find(prop => prop.id === propertyId);
    if(!property){
      throw new Error("Property named \"" + propertyId + "\" was not found");
    }
    return this.dataView.dataTable.getCellText(row, property);
  }
}

