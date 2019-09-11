import { getDataViewLifecycle } from "model/selectors/DataView/getDataViewLifecycle"

export function navigateAsChild(ctx: any) {
  return function navigateAsChild() {
    getDataViewLifecycle(ctx).navigateAsChild();
  }
}