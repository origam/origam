import {DataViewCache} from "../../entities/DataViewCache";
import {getFormScreen} from "./getFormScreen";

export function getDataViewCache(ctx: any): DataViewCache {
  return getFormScreen(ctx).dataViewCache;
}