import { IOpenedScreen, IDialogInfo } from "../entities/types/IOpenedScreen";
import { OpenedScreen } from "../entities/OpenedScreen";
import { IFormScreen } from "../entities/types/IFormScreen";
import { IMainMenuItemType } from "../entities/types/IMainMenu";

export function createOpenedScreen(
  menuItemId: string,
  menuItemType: IMainMenuItemType,
  order: number,
  title: string,
  content: IFormScreen,
  dontRequestData: boolean,
  dialogInfo?: IDialogInfo,
): IOpenedScreen {
  return new OpenedScreen({
    menuItemId,
    menuItemType,
    order,
    title,
    content,
    dialogInfo,
    dontRequestData
  });
}
