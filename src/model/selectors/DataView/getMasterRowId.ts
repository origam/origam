import {getBindingRoot} from './getBindingRoot';

export function getMasterRowId(ctx: any) {
  return getBindingRoot(ctx).selectedRowId;
}