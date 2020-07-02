import {getIsDataViewWorking} from "./getIsDataViewWorking";
import {getIsFormScreenWorking} from "../FormScreen/getIsFormScreenWorking";

export function getIsDataViewOrFormScreenWorking(ctx: any) {
  return getIsDataViewWorking(ctx) || getIsFormScreenWorking(ctx);
}
