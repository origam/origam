import {getFormScreen} from '../FormScreen/getFormScreen';

export function getDataViewsByEntity(ctx: any, entity: string) {
  return getFormScreen(ctx).getDataViewsByEntity(entity);
}