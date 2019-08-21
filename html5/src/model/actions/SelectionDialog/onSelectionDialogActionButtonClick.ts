import { IAction } from "model/entities/types/IAction";

export function onSelectionDialogActionButtonClick(ctx: any) {
  return function onSelectionDialogActionButtonClick(
    event: any,
    action: IAction
  ) {
    console.log("Act:", action);
  };
}
