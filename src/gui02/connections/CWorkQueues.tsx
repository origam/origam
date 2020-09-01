import React from "react";
import {IWorkbench} from "model/entities/types/IWorkbench";
import {MobXProviderContext, observer} from "mobx-react";
import {getWorkQueuesItems} from "model/selectors/WorkQueues/getWorkQueuesItems";
import {WorkQueuesItem} from "gui02/components/WorkQueues/WorkQueuesItem";
import {computed} from "mobx";
import {Icon} from "gui02/components/Icon/Icon";
import {onWorkQueuesListItemClick} from "model/actions-ui/WorkQueues/onWorkQueuesListItemClick";
import {getActiveScreen} from "model/selectors/getActiveScreen";

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
              isEmphasized={item.countTotal > 0}
              isOpenedScreen={this.workbench.openedScreenIdSet.has(item.id)}
              isActiveScreen={activeMenuItemId === item.id}
              icon={<Icon src="./icons/work-queue.svg" tooltip={item.name}/>}
              label={
                <>
                  {item.name}
                  {item.countTotal > 0 && <> ({item.countTotal})</>}
                </>
              }
              onClick={event => onWorkQueuesListItemClick(this.workbench)(event, item)}
            />
          );
        })}
      </>
    );
  }
}
