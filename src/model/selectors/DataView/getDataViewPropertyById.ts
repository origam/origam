import {getDataView} from "./getDataView";

export function getDataViewPropertyById(ctx: any, id: string) {
  return getDataView(ctx).properties
    .flatMap(property => [property, ...property.childProperties])
    .find(item => item.id === id);
}
