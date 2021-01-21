import { SearchDialog } from "gui/Components/Dialogs/SearchDialog";
import { ISearchResult } from "model/entities/types/ISearchResult";
import { getDialogStack } from "model/selectors/getDialogStack";
import { getWorkbench } from "model/selectors/getWorkbench";
import React from "react";

export function openSearchWindow(ctx: any) {

  const sidebarState = getWorkbench(ctx).sidebarState;

  const closeDialog = getDialogStack(ctx).pushDialog(
    "",
    <SearchDialog 
      ctx={ctx} 
      onCloseClick={() => closeDialog()}
      onSearchResultsChange={(results: ISearchResult[]) => sidebarState.onSearchResultsChange(results)}
    />,
    undefined,
    true
  );
}

