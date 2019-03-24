import { IModel } from "../types/IModel";
import { createModel } from "./buildModel";
import { IDataViewParam } from "../types/ModelParam";

export class ScreenFactory {
  getScreen(modelParam: IDataViewParam[]): IModel {
    return createModel(modelParam);
  }
}