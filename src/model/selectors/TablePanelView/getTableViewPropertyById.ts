import {getTablePanelView} from "./getTablePanelView";

export function getTableViewPropertyById(ctx: any, id: string) {
  const property = getTablePanelView(ctx).propertyMap.get(id);
  if(!property){
    throw new Error("No property with id: \" "+id+" \" found ")
  }
  return property
}
