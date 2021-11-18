import {getIsDataViewWorking} from "./getIsDataViewWorking";
import {getIsFormScreenWorkingDelayed} from "../FormScreen/getIsFormScreenWorking";

export function getIsDataViewOrFormScreenWorkingDelayed(ctx: any) {
  return getIsDataViewWorking(ctx) || getIsFormScreenWorkingDelayed(ctx);
}
