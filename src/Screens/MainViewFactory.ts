import { IMainViewFactory, IMainView, IMainViews } from "./types";
import { ICommandType } from "../MainMenu/MainMenu";
import { FormScreen } from "./FormScreen/FormScreen";
import { ML } from "../utils/types";
import { createFormScreen } from "./FormScreen/factory";
import { IApi } from "../Api/IApi";

export class MainViewFactory implements IMainViewFactory {
  constructor(public P: { mainViews: ML<IMainViews>; api: ML<IApi> }) {}

  create(
    menuItemId: string,
    order: number,
    itemType: ICommandType,
    menuItemLabel: string
  ): IMainView {
    console.log("Creating main view:", menuItemId, order, itemType);
    return createFormScreen({
      menuItemId,
      menuItemLabel,
      order,
      mainViews: this.P.mainViews,
      api: this.P.api
    });
  }
}
