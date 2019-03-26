import {
  IModel,
  IDataViewQuery,
} from "../types/IModel";

import { IDataTable } from "./data/types/IDataTable";
import { ICursor } from "./cursor/types/ICursor";
import { IDataViews } from "./specificViews/types/IDataViews";

export class Model implements IModel {
  constructor(public dataViews: IDataViews[]) {}

  getDataViews(query: IDataViewQuery): IDataViews| undefined {
    if (query.dataViewId) {
      return this.dataViews.find(obj => obj.id === query.dataViewId);
    }
    return;
  }

}
