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
import { WorkQueuesItem } from "gui/Components/WorkQueues/WorkQueuesItem";
import { flow } from "mobx";
import { Icon } from "gui/Components/Icon/Icon";
import { getActiveScreen } from "model/selectors/getActiveScreen";
import { getChatrooms } from "model/selectors/Chatrooms/getChatrooms";
import { onChatroomsListItemClick } from "model/actions/Chatrooms/onChatroomsListItemClick";
import { openNewUrl } from "model/actions/Workbench/openNewUrl";
import { IUrlOpenMethod } from "model/entities/types/IUrlOpenMethod";
import { T } from "utils/translation";

@observer
export class CChatSection extends React.Component {
  static contextType = MobXProviderContext;

  get workbench(): IWorkbench {
    return this.context.workbench;
  }

  get sortedItems() {
    return getChatrooms(this.workbench).items;
  }

  render() {
    return (
      <>
        <WorkQueuesItem
          key={"new_chat"}
          isEmphasized={false}
          isOpenedScreen={false}
          isActiveScreen={false}
          icon={<Icon src="./icons/add.svg" tooltip={T("New Chat", "new_chat")}/>}
          label={<>{T("New Chat", "new_chat")}</>}
          onClick={(event) => {
            const self = this;
            flow(function*() {
              yield*openNewUrl(self.workbench)(
                `chatrooms/index.html#/chatroom`,
                IUrlOpenMethod.OrigamTab,
                "New Chat"
              );
            })();
          }}
        />
        {this.sortedItems.map((item) => {
          const activeScreen = getActiveScreen(this.workbench);
          const activeMenuItemId = activeScreen ? activeScreen.menuItemId : undefined;
          return (
            <WorkQueuesItem
              key={item.id}
              isEmphasized={item.unreadMessageCount > 0}
              isOpenedScreen={this.workbench.openedScreenIdSet.has(item.id)}
              isActiveScreen={activeMenuItemId === item.id}
              icon={<Icon src="./icons/chat.svg" tooltip={item.topic}/>}
              label={
                <>
                  {item.topic}
                  {item.unreadMessageCount > 0 && <> ({item.unreadMessageCount})</>}
                </>
              }
              onClick={(event) => onChatroomsListItemClick(this.workbench)(event, item)}
              id={item.id}
            />
          );
        })}
      </>
    );
  }
}
