import { flow } from "mobx";
import { getProperty } from "model/selectors/DataView/getProperty";
import { getDataViewPropertyById } from "model/selectors/DataView/getDataViewPropertyById";

export function onColumnWidthChanged(ctx: any) {
  return flow(function* onColumnWidthChanged(id: string, width: number) {
    const prop = getDataViewPropertyById(ctx, id);
    if(prop) {
      prop.setColumnWidth(width);
    }
  });
}