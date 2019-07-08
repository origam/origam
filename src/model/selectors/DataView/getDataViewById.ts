import { IDataView } from "../../types/IDataView";
import { getOpenedScreen } from "../getOpenedScreen";
import { IFormScreen, ILoadedFormScreen } from "../../types/IFormScreen";

export function getDataViewById(ctx: any, id: string): IDataView | undefined {
  return (getOpenedScreen(ctx).content as ILoadedFormScreen).dataViews.find(
    item => item.id === id
  );
}
