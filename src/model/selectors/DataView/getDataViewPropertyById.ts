import {getDataView} from "./getDataView";

export function getDataViewPropertyById(ctx: any, id: string) {
  return getDataView(ctx).properties.find(item => item.id === id);
}
