import { getDataTable } from "../DataView/getDataTable";
import { getTableViewProperties } from "./getTableViewProperties";

export function getCellValue(ctx: any) {
  const dataTable = getDataTable(ctx);
  const tableViewProperties = getTableViewProperties(ctx);
  return function getCellValue(rowIndex: number, columnIndex: number) {
    const prop = tableViewProperties[columnIndex];
    const value = dataTable.rows[rowIndex][prop.dataIndex];
    return value;
  };
}
