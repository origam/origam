import { SearchDialog, SEARCH_DIALOG_KEY } from "gui/Components/Dialogs/SearchDialog";
import { getDialogStack } from "model/selectors/getDialogStack";
import { getSearcher } from "model/selectors/getSearcher";
import { getWorkbench } from "model/selectors/getWorkbench";
import React from "react";

export function openSearchWindow(ctx: any) {
  getSearcher(ctx).clear();

  const closeDialog = getDialogStack(ctx).pushDialog(
    SEARCH_DIALOG_KEY,
    <SearchDialog 
      ctx={ctx} 
      onCloseClick={() => closeDialog()}
    />,
    undefined,
    true
  );
}

export function isGlobalAutoFocusDisabled(ctx: any){
  return getDialogStack(ctx).isOpen(SEARCH_DIALOG_KEY);
}
