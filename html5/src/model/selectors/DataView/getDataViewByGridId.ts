import { getOpenedScreen } from "../getOpenedScreen";
import { ILoadedFormScreen } from "model/entities/types/IFormScreen";
import { getGridId } from "./getGridId";

export function getDataViewByGridId(ctx: any, gridId: string) {
  return (getOpenedScreen(ctx).content as ILoadedFormScreen).dataViews.find(
    item => getGridId(item) === gridId
  );
}
