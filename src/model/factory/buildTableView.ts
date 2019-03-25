
import { TableView  } from "../entities/specificView/table/TableView";
import { Cursor } from "../entities/cursor/Cursor";
import { DataTable } from "../entities/data/DataTable";
import { Records } from "../entities/data/Records";
import { Properties } from "../entities/data/Properties";
import { buildProperties } from "./buildProperties";
import { PropertiesReordering } from "../entities/specificView/table/PropertiesReordering";
import { Record } from "../entities/data/Record";
import { IDataViewParam } from "../types/ModelParam";

export function buildTableView(param: IDataViewParam) {
  const properties = buildProperties(param.properties);
  const records = new Records();
  const dataTable = new DataTable({
    records,
    properties
  });
  const cursor = new Cursor(dataTable);
  const tableView = new TableView({
    ...param,
    cursor,
    dataTable,
    properties: new PropertiesReordering(
      properties,
      properties.items.filter(prop => prop.id !== "Id").map(prop => prop.id)
    )
  });


  return tableView;
}
