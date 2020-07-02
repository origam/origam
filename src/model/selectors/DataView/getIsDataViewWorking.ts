import {getDataView} from "./getDataView";

export function getIsDataViewWorking(ctx: any) {
  return getDataView(ctx).isWorking;
}
