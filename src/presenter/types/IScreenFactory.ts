import * as ScreenInfBp from "./IInfScreenBlueprints";
import { IScreen } from "./IScreenPresenter";

export interface IScreenFactory {
  getScreen(blueprint: ScreenInfBp.IScreen): IScreen;
}