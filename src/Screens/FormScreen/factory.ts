import { IApi } from "../../Api/IApi";
import { ML } from "../../utils/types";
import { IMainViews } from "../types";
import { FormScreen } from "./FormScreen";
import { FormScreenMachine } from "./FormScreenMachine";
import { ScreenContentFactory } from "./ScreenContentFactory";

export function createFormScreen(P: {
  menuItemId: string;
  order: number;
  menuItemLabel: string;
  mainViews: ML<IMainViews>;
  api: ML<IApi>;
}): FormScreen {
  const formScreen: FormScreen = new FormScreen({
    menuItemId: P.menuItemId,
    order: P.order,
    menuItemLabel: P.menuItemLabel,
    mainViews: P.mainViews,
    machine: () => screenMachine
  });

  const screenMachine: FormScreenMachine = new FormScreenMachine({
    menuItemId: P.menuItemId,
    formScreen: () => formScreen,
    screenContentFactory: () => screenContentFactory,
    api: P.api
  });
  const screenContentFactory = new ScreenContentFactory({
    api: P.api,
    menuItemId: P.menuItemId
  });
  return formScreen;
}
