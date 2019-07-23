import { getDataViewLifecycle } from "../../selectors/DataView/getDataViewLifecycle";

export function loadFreshData(ctx: any) {
  getDataViewLifecycle(ctx).loadFresh()
}