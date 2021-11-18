import {IDataView} from "../../entities/types/IDataView";
import {getOpenedScreen} from "../getOpenedScreen";

export function getDataViewByModelInstanceId(ctx: any, modelInstanceId: string): IDataView | undefined {
  return getOpenedScreen(ctx).content.formScreen!.dataViews.find(
    item => item.modelInstanceId === modelInstanceId
  );
}