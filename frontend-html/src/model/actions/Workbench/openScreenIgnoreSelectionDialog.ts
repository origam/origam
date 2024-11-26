/*
Copyright 2005 - 2024 Advantage Solutions, s. r. o.

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

import selectors from "model/selectors-tree";
import { produce } from "immer";
import {
  onMainMenuItemClick
} from "model/actions-ui/MainMenu/onMainMenuItemClick";

export function* openScreenIgnoreSelectionDialog(menuId: string, referenceId: string, ctx: any) {
  let menuItem = menuId && selectors.mainMenu.getItemById(ctx, menuId);
  if (menuItem) {
    menuItem = {...menuItem, parent: undefined, elements: []};
    menuItem = produce(menuItem, (draft: any) => {
      if (menuItem.attributes.type.startsWith("FormReferenceMenuItem")) {
        draft.attributes.type = "FormReferenceMenuItem";
      }
      draft.attributes.lazyLoading = "false";
    });

    yield onMainMenuItemClick(ctx)({
      event: undefined,
      item: menuItem,
      idParameter: referenceId,
      isSingleRecordEdit: true,
    });
  }
}