import { TableView } from "../entities/specificView/table/TableView";
import { Cursor } from "../entities/cursor/Cursor";
import { DataTable } from "../entities/data/DataTable";
import { Records } from "../entities/data/Records";
import { Properties } from "../entities/data/Properties";
import { buildProperties } from "./buildProperties";
import { IDataViewParam } from "../types/ModelParam";

export function buildTableView(param: IDataViewParam) {
  const properties = buildProperties(param.properties);
  const reorderedProperties = new Properties(
    properties.items,
    properties.items.map(item => item.id).filter(id => id !== "Id")
  )
  const records = new Records();
  const dataTable = new DataTable({
    records,
    properties
  });
  const cursor = new Cursor(dataTable, reorderedProperties);
  const tableView = new TableView({
    ...param,
    cursor,
    dataTable,
    properties,
    reorderedProperties
  });

  return tableView;
}
