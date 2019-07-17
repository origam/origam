import { getFormScreen } from '../FormScreen/getFormScreen';
export function getDataViewByEntity(ctx: any, entity: string) {
  return getFormScreen(ctx).getDataViewByEntity(entity);
}