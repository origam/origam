import {getApplication} from "./getApplication";
import {IApi} from "../entities/types/IApi";

export function getApi(ctx: any): IApi {
  return getApplication(ctx).api;
}
