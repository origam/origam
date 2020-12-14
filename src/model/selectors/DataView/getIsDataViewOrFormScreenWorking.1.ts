import { getIsDataViewWorking } from "./getIsDataViewWorking";
import { getFormScreenLifecycle } from "../FormScreen/getFormScreenLifecycle";


export function getIsDataViewOrFormScreenWorking(ctx: any) {
  return getIsDataViewWorking(ctx) || getFormScreenLifecycle(ctx).isWorking;
}
