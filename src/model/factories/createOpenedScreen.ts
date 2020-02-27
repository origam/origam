import { IOpenedScreen, IDialogInfo } from "../entities/types/IOpenedScreen";
import { OpenedScreen } from "../entities/OpenedScreen";
import { IFormScreen, IFormScreenEnvelope } from "../entities/types/IFormScreen";
import { IMainMenuItemType } from "../entities/types/IMainMenu";

export function createOpenedScreen(
  menuItemId: string,
  menuItemType: IMainMenuItemType,
  order: number,
  title: string,
  content: IFormScreenEnvelope,
  dontRequestData: boolean,
  dialogInfo: IDialogInfo | undefined,
  parameters: { [key: string]: any },
  isSleeping?: boolean,
  isSleepingDirty?: boolean
): IOpenedScreen {
  return new OpenedScreen({
    menuItemId,
    menuItemType,
    order,
    title,
    content,
    dialogInfo,
    dontRequestData,
    parameters,
    isSleeping,
    isSleepingDirty
  });
}
