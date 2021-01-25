import { SearchDialog, SEARCH_DIALOG_KEY } from "gui/Components/Dialogs/SearchDialog";
import { getDialogStack } from "model/selectors/getDialogStack";
import { getSearcher } from "model/selectors/getSearcher";
import { getWorkbench } from "model/selectors/getWorkbench";
import React from "react";

export function openSearchWindow(ctx: any) {
  const sidebarState = getWorkbench(ctx).sidebarState;
  getSearcher(ctx).clear();

  const closeDialog = getDialogStack(ctx).pushDialog(
    SEARCH_DIALOG_KEY,
    <SearchDialog 
      ctx={ctx} 
      onCloseClick={() => closeDialog()}
      onSearchResultsChange={(groups) => sidebarState.onSearchResultsChange(groups)}
    />,
    undefined,
    true
  );
}

export function isGlobalAutoFocusDisabled(ctx: any){
  return getDialogStack(ctx).isOpen(SEARCH_DIALOG_KEY);
}
