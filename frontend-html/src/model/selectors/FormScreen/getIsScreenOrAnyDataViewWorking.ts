import {getDataViewList} from "./getDataViewList";
import {getIsDataViewWorking} from "../DataView/getIsDataViewWorking";
import {getIsFormScreenWorkingDelayed} from "./getIsFormScreenWorking";

export function getIsScreenOrAnyDataViewWorking(ctx: any) {
  return (
    getIsFormScreenWorkingDelayed(ctx) ||
    Array.from(getDataViewList(ctx)).some(dataView =>
      getIsDataViewWorking(dataView)
    )
  );
}
