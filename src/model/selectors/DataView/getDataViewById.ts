import { IDataView } from "../../entities/types/IDataView";
import { getOpenedScreen } from "../getOpenedScreen";
import { IFormScreen, ILoadedFormScreen } from "../../entities/types/IFormScreen";

export function getDataViewById(ctx: any, id: string): IDataView | undefined {
  return (getOpenedScreen(ctx).content as ILoadedFormScreen).dataViews.find(
    item => item.id === id
  );
}
