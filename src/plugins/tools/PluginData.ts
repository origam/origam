import {IPluginData} from "../types/IPluginData";
import {IDataView} from "../../model/entities/types/IDataView";
import {IPluginRow, IPluginTableRow} from "../types/IPluginRow";
import {IGroupRow, ITableRow} from "../../gui/Components/ScreenElements/Table/TableRendering/types";
import {getDataTable} from "../../model/selectors/DataView/getDataTable";
import {getProperties} from "../../model/selectors/DataView/getProperties";


export function createPluginData(dataView: IDataView): IPluginData | undefined {
  if(!dataView){
    return undefined;
  }
  return {
    dataView: {
      tableRows:dataView.tableRows.map(row => Array.isArray(row) ?
        rowToPluginRow(row, dataView)
        : groupRowToPluginGroupRow(row)
      )
    }
  }
}


function rowToPluginRow(row: any[], ctx: any): IPluginRow {
  let dataTable = getDataTable(ctx);
  let properties = getProperties(ctx);
  return properties
    .reduce(
      (rowObj:{[key:string]:any}, property)=> {rowObj[property.id] = dataTable.getCellValue(row, property); return rowObj},
      {}
    )
}

function groupRowToPluginGroupRow(groupRow: IGroupRow){
  return groupRow;
}

