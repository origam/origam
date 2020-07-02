import {getTableViewProperties} from './getTableViewProperties';

export function getColumnCount(ctx: any) {
  return getTableViewProperties(ctx).length;
}