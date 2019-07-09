import { getProperties } from './getProperties';

export function getColumnNamesToLoad(ctx: any): string[] {
  return getProperties(ctx).map(prop => prop.id);
}