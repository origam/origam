import { buildTableView } from "./buildTableView";

import { Model } from "../entities/Model";
import { IDataViewParam } from "../types/ModelParam";

export function createModel(modelParam: IDataViewParam[]) {
  const tableViews = modelParam.map(mp => buildTableView(mp));
  const model = new Model(tableViews);
  return model;
}
