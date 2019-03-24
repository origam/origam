
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

  if (param.id === "AsPanel9_30") {
    for (let i = 0; i < 10; i++) {
      const row = [];
      for (let j = 0; j < properties.count; j++) {
        row.push(`${i} & ${j}`);
      }
      records.items.push(
        new Record({
          id: row[0],
          values: row,
          dirtyValues: new Map()
        })
      );
    }

    dataTable.deleteRecordById("7 & 0");
    dataTable.setDirtyValueById("3 & 0", "refBusinessPartnerId", "DIRTY_VALUE");
    cursor.selectCell("4 & 0", "refBusinessPartnerId");
  }

  return tableView;
}
