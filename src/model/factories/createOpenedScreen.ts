import {IDialogInfo, IOpenedScreen} from "../entities/types/IOpenedScreen";
import {OpenedScreen} from "../entities/OpenedScreen";
import {IFormScreenEnvelope} from "../entities/types/IFormScreen";
import {IMainMenuItemType} from "../entities/types/IMainMenu";
import {WebScreen} from "model/entities/WebScreen";
import {getApi} from "model/selectors/getApi";

export function *createOpenedScreen(
  ctx: any,
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
): Generator {
  if(menuItemType ===  IMainMenuItemType.ReportReferenceMenuItem){
    const api = getApi(ctx);
    const url = (yield api.getReportFromMenu({menuId: menuItemId})) as string;
    return new WebScreen(
      title,
      url,
      menuItemId,
      order
    );
  }
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
