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

import { DialogScreen } from "gui/Workbench/ScreenArea/ScreenArea";
import { MobXProviderContext, observer } from "mobx-react";
import { getOpenedDialogScreenItems } from "model/selectors/getOpenedDialogScreenItems";
import React from "react";
import { getNewRecordScreenButtons } from "gui/connections/NewRecordScreen";

@observer
export class CDialogContent extends React.Component {
  static contextType = MobXProviderContext;

  get workbench() {
    return this.context.workbench;
  }

  render() {
    const openedDialogItems = getOpenedDialogScreenItems(this.workbench);
    return (
      <>
        {openedDialogItems.map(item =>
          <DialogScreen
            openedScreen={item}
            bottomButtons={item.isNewRecordScreen ? getNewRecordScreenButtons(item) : null}
            showCloseButton={!item.isNewRecordScreen}
            key={`${item.menuItemId}@${item.order}`}
          />
        )}
      </>
    );
  }
}
