import { IApi } from "../Api/IApi";
import { FormScreen } from "../Screens/FormScreen/FormScreen";
import { FormScreenMachine } from "../Screens/FormScreen/FormScreenMachine";
import { IFormScreen, IFormScreenMachine } from "../Screens/FormScreen/types";
import { ML } from "../utils/types";
import { IFormScreenScope } from "./types/IFormScreenScope";
import { IMainViews } from "../Screens/types";
import { ScreenContentFactory } from "../Screens/FormScreen/ScreenContentFactory";

export class FormScreenScope implements IFormScreenScope {
  constructor(
    public P: {
      menuItemId: string;
      order: number;
      menuItemLabel: string;
      mainViews: ML<IMainViews>;
      api: ML<IApi>;
    }
  ) {}

  formScreen: IFormScreen = new FormScreen({
    menuItemId: this.P.menuItemId,
    order: this.P.order,
    menuItemLabel: this.P.menuItemLabel,
    mainViews: this.P.mainViews,
    machine: () => this.screenMachine,
    api: this.P.api
  });

  screenMachine: IFormScreenMachine = new FormScreenMachine({
    menuItemId: this.P.menuItemId,
    formScreen: () => this.formScreen,
    screenContentFactory: () => this.screenContentFactory,
    api: this.P.api
  });
  screenContentFactory = new ScreenContentFactory({
    formScreen: this.formScreen
  });
}
