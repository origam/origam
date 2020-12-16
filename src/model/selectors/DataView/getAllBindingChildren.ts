import { IDataView } from "model/entities/types/IDataView";
import { getBindingChildren } from "./getBindingChildren";

export function getAllBindingChildren(ctx: any) {
    const resultArray: IDataView[] = [];
    getChildrenRecursive(ctx, resultArray);
    return resultArray;
}

function getChildrenRecursive(ctx: any, resultArray: IDataView[]) {
  const childDatViews = getBindingChildren(ctx);
  for (const dataView of childDatViews) {
    resultArray.push(dataView);
    getChildrenRecursive(dataView, resultArray);
  }
}