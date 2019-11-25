import { getDataViewList } from "./getDataViewList";
import { getIsDataViewWorking } from "../DataView/getIsDataViewWorking";
import { getIsFormScreenWorking } from "./getIsFormScreenWorking";

export function getIsScreenOrAnyDataViewWorking(ctx: any) {
  return (
    getIsFormScreenWorking(ctx) ||
    Array.from(getDataViewList(ctx)).some(dataView =>
      getIsDataViewWorking(dataView)
    )
  );
}
