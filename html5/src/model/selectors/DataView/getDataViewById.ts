import { IDataView } from "../../entities/types/IDataView";
import { getOpenedScreen } from "../getOpenedScreen";

export function getDataViewById(ctx: any, id: string): IDataView | undefined {
  return getOpenedScreen(ctx).content.formScreen!.dataViews.find(
    item => item.id === id
  );
}
