import React from "react";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { MobXProviderContext, observer } from "mobx-react";
import { getWorkQueuesItems } from "model/selectors/WorkQueues/getWorkQueuesItems";
import { WorkQueuesItem } from "gui02/components/WorkQueues/WorkQueuesItem";
import { computed, flow } from "mobx";
import { Icon } from "gui02/components/Icon/Icon";
import { onWorkQueuesListItemClick } from "model/actions-ui/WorkQueues/onWorkQueuesListItemClick";
import { getActiveScreen } from "model/selectors/getActiveScreen";
import { getChatrooms } from "model/selectors/Chatrooms/getChatrooms";
import { onChatroomsListItemClick } from "model/actions/Chatrooms/onChatroomsListItemClick";
import { openNewUrl } from "model/actions/Workbench/openNewUrl";
import { IUrlUpenMethod } from "model/entities/types/IUrlOpenMethod";

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
          isEmphasized={false}
          isOpenedScreen={false}
          isActiveScreen={false}
          icon={<Icon src="./icons/add.svg" tooltip={"New chatroom"} />}
          label={<>Create new chatroom</>}
          onClick={(event) => {
            const _this = this;
            flow(function* () {
              yield* openNewUrl(_this.workbench)(
                `chatrooms/index.html#/chatroom`,
                IUrlUpenMethod.OrigamTab,
                "New Chatroom"
              );
            })();
          }}
        />
        {this.sortedItems.map((item) => {
          const activeScreen = getActiveScreen(this.workbench);
          const activeMenuItemId = activeScreen ? activeScreen.menuItemId : undefined;
          return (
            <WorkQueuesItem
              isEmphasized={item.unreadMessageCount > 0}
              isOpenedScreen={this.workbench.openedScreenIdSet.has(item.id)}
              isActiveScreen={activeMenuItemId === item.id}
              icon={<Icon src="./icons/work-queue.svg" tooltip={item.topic} />}
              label={
                <>
                  {item.topic}
                  {item.unreadMessageCount > 0 && <> ({item.unreadMessageCount})</>}
                </>
              }
              onClick={(event) => onChatroomsListItemClick(this.workbench)(event, item)}
            />
          );
        })}
      </>
    );
  }
}
