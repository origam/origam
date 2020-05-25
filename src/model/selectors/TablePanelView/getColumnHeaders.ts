import { getTableViewProperties } from "./getTableViewProperties";
import { IColumnHeader } from './types';
import { getPropertyOrdering } from "../DataView/getPropertyOrdering";


export function getColumnHeaders(ctx: any): IColumnHeader[] {
  const tableViewProperties = getTableViewProperties(ctx);
  return tableViewProperties.map(prop => {
    const ordering = getPropertyOrdering(ctx, prop.id);
    return {
      label: prop.name,
      id: prop.id,
      ordering: ordering.ordering,
      order: ordering.order
    }
  })
}