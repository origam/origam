import {getOpenedScreen} from "../getOpenedScreen";
import {getGridId} from "./getGridId";

export function getDataViewByGridId(ctx: any, gridId: string) {
  return getOpenedScreen(ctx).content.formScreen!.dataViews.find(
    item => getGridId(item) === gridId
  );
}
