import {IDialogInfo} from "../entities/types/IOpenedScreen";
import {OpenedScreen} from "../entities/OpenedScreen";
import {IFormScreenEnvelope} from "../entities/types/IFormScreen";
import {IMainMenuItemType} from "../entities/types/IMainMenu";

export function *createOpenedScreen(
  ctx: any,
  menuItemId: string,
  menuItemType: IMainMenuItemType,
  order: number,
  title: string,
  content: IFormScreenEnvelope,
  lazyLoading: boolean,
  dialogInfo: IDialogInfo | undefined,
  parameters: { [key: string]: any },
  isSleeping?: boolean,
  isSleepingDirty?: boolean
): Generator {
  return new OpenedScreen({
    menuItemId,
    menuItemType,
    order,
    title,
    content,
    dialogInfo,
    lazyLoading,
    parameters,
    isSleeping,
    isSleepingDirty
  });
}
