import { IOpenedScreens } from "./types/IOpenedScreens";
import { IOpenedScreen } from "./types/IOpenedScreen";

export class OpenedScreens implements IOpenedScreens {
  items: IOpenedScreen[] = [];
}
