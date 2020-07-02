import {getFormScreen} from '../FormScreen/getFormScreen';

export function getDataSourceByEntity(ctx: any, entity: string) {
  return getFormScreen(ctx).getDataSourceByEntity(entity);
}