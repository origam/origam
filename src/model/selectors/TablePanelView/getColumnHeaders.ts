import { getTableViewProperties } from "./getTableViewProperties";
import { IColumnHeader, IOrderByDirection } from './types';


export function getColumnHeaders(ctx: any): IColumnHeader[] {
  const tableViewProperties = getTableViewProperties(ctx);
  return tableViewProperties.map(prop => {
    return {
      label: prop.name,
      id: prop.id,
      ordering: IOrderByDirection.ASC
    }
  })
}