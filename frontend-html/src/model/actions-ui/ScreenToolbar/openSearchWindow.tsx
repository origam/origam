/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { SearchDialog } from "gui/Components/Dialogs/SearchDialog";
import { getDialogStack } from "model/selectors/getDialogStack";
import { getSearcher } from "model/selectors/getSearcher";
import React from "react";
import { SEARCH_DIALOG_KEY } from "gui/Components/Search/SearchView";

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

export function isGlobalAutoFocusDisabled(ctx: any) {
  return getDialogStack(ctx).isOpen(SEARCH_DIALOG_KEY);
}
