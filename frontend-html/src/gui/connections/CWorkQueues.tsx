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

import React from "react";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { MobXProviderContext, observer } from "mobx-react";
import { getWorkQueuesItems } from "model/selectors/WorkQueues/getWorkQueuesItems";
import { WorkQueuesItem } from "gui/Components/WorkQueues/WorkQueuesItem";
import { computed } from "mobx";
import { Icon } from "gui/Components/Icon/Icon";
import { onWorkQueuesListItemClick } from "model/actions-ui/WorkQueues/onWorkQueuesListItemClick";
import { getActiveScreen } from "model/selectors/getActiveScreen";

@observer
export class CWorkQueues extends React.Component {
  static contextType = MobXProviderContext;

  get workbench(): IWorkbench {
    return this.context.workbench;
  }

  @computed get sortedItems() {
    const workQueueItems = [...getWorkQueuesItems(this.workbench)];
    workQueueItems.sort((a, b) => (a.name || "").localeCompare(b.name || ""));
    return workQueueItems;
  }

  render() {
    return (
      <>
        {this.sortedItems.map(item => {
          const activeScreen = getActiveScreen(this.workbench);
          const activeMenuItemId = activeScreen ? activeScreen.menuItemId : undefined;
          return (
            <WorkQueuesItem
              key={item.id}
              isEmphasized={item.countTotal > 0}
              isOpenedScreen={this.workbench.openedScreenIdSet.has(item.id)}
              isActiveScreen={activeMenuItemId === item.id}
              icon={<Icon src="./icons/work-queue.svg" tooltip={item.name}/>}
              tooltip={item.name}
              label={
                <>
                  {item.name}
                  {item.countTotal > 0 && <> ({item.countTotal})</>}
                </>
              }
              id={item.id}
              onClick={event => onWorkQueuesListItemClick(this.workbench)(event, item)}
            />
          );
        })}
      </>
    );
  }
}
