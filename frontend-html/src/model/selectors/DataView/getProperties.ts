import {IProperty} from "../../entities/types/IProperty";
import {getDataView} from "./getDataView";

export function getProperties(ctx: any): IProperty[] {
  return getDataView(ctx).properties;
}
