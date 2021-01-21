import { SearchDialog } from "gui/Components/Dialogs/SearchDialog";
import { getDialogStack } from "model/selectors/getDialogStack";
import React from "react";

export function openSearchWindow(ctx: any) {
  const closeDialog = getDialogStack(ctx).pushDialog(
    "",
    <SearchDialog
      onCloseClick={() => {
        closeDialog();
      }}
    />
  );
}

