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

import { Icon } from "gui/Components/Icon/Icon";
import { useContext } from "react";
import { MobXProviderContext, observer } from "mobx-react";
import { CWorkQueues } from "gui/connections/CWorkQueues";
import { CSidebarInfoSection } from "gui/connections/CSidebarInfoSection";
import { T } from "utils/translation";
import { SearchResults } from "gui/Components/Search/SearchResults";
import { CFavorites, onDragEndAction } from "gui/connections/CFavorites";
import { getFavorites } from "model/selectors/MainMenu/getFavorites";
import { MenuItemList } from "../MenuItemList";
import S from "gui/connections/MobileComponents/MobileSideBar.module.scss";
import { getShowChat } from "model/selectors/PortalSettings/getShowChat";
import { getShowWorkQues } from "model/selectors/PortalSettings/getShowWorkQues";
import { DragDropContext } from "react-beautiful-dnd";
import { getWorkQueuesTotalItemsCount } from "model/selectors/WorkQueues/getWorkQueuesTotalItemCount";
import { SidebarAlertCounter } from "gui/Components/Sidebar/AlertCounter";
import { CChatSection } from "../CChatSection";
import { IWorkbench } from "model/entities/types/IWorkbench";
import { SidebarState } from "model/entities/SidebarState";
import { getChatrooms } from "model/selectors/Chatrooms/getChatrooms";


export const MobileSideBar = observer( () => {
  const mobxContext = useContext(MobXProviderContext);
  const workbench = mobxContext.workbench as IWorkbench;
  const sidebarState = workbench.sidebarState;
  const application = mobxContext.application;
  const favorites = getFavorites(workbench);
  const showChat = getShowChat(workbench);
  const showWorkQues = getShowWorkQues(workbench);
  const workQueuesItemsCount = getWorkQueuesTotalItemsCount(workbench);
  const totalUnreadMessages = getChatrooms(workbench).totalItemCount;

  function renderContent() {
    switch (sidebarState.activeSection) {
      case "Menu":
        return <MenuItemList ctx={application} />;
      case "WorkQueues":
        return <CWorkQueues />;
      case "Favorites":
        return ( 
          <DragDropContext onDragEnd={(result) => onDragEndAction(result, workbench)}>
          {favorites.favoriteFolders
            .map((folder) => (
              <CFavorites
                key={folder.id}
                ctx={workbench}
                folder={folder}
                isActive={true}
                forceOpen={true}/>
            ))
            }
          </DragDropContext>
        );
      case "Search":
        return (
          <SearchResults groups={sidebarState.searchResultGroups} ctx={workbench}/>
        );
      case "Chat":
        return <CChatSection/>;
      default:
        return <MenuItemList ctx={application} />;
    }
  }

  return (

    <div className={S.root}>
      <div className={S.contentContainer}>
        {renderContent()}
      </div>
      <div className={S.bottomContainer}>
        <Icon 
          className={getSectionIconClass("Menu", sidebarState)}
          src="./icons/menu.svg" 
          tooltip={T("Menu", "menu")}
          onClick={() => sidebarState.activeSection = "Menu"}
        />
        { showWorkQues &&
          <SectionButton
            sectionName="WorkQueues"
            iconPath="./icons/work-queue.svg"
            tooltip={T("Work Queues", "work_queue_measure")}
            onClick={() => sidebarState.activeSection = "WorkQueues"}
            itemCount={workQueuesItemsCount}
            sidebarState={sidebarState}
          />
        }
        { showChat &&
          <SectionButton
            sectionName="Chat"
            iconPath="./icons/chat.svg"
            tooltip={T("Chat", "chat")}
            onClick={() => sidebarState.activeSection = "Chat"}
            itemCount={totalUnreadMessages}
            sidebarState={sidebarState}
          />
        }
        <Icon
          className={getSectionIconClass("Search", sidebarState)}
          src="./icons/search.svg"
          tooltip={T("Search", "search_result", sidebarState.resultCount)}
          onClick={() => sidebarState.activeSection = "Search"}
        />
        <Icon 
          className={getSectionIconClass("Favorites", sidebarState)}
          src="./icons/favorites.svg" 
          tooltip={T("Search", "search_result", sidebarState.resultCount)}
          onClick={() => sidebarState.activeSection = "Favorites"}
        />
      </div>
    </div>
  );
});

const SectionButton = (props: {
  sectionName: string,
  sidebarState: SidebarState,
  iconPath: string,
  tooltip: string,
  itemCount: number,
  onClick: () => void
}) => {
  return(
    <div 
      className={S.workQueueButton}
      onClick={() => props.sidebarState.activeSection = props.sectionName}>
      <Icon
        className={getSectionIconClass(props.sectionName, props.sidebarState)}
        src={props.iconPath}
        tooltip={props.tooltip}
        />
        {props.itemCount > 0 && (
          <SidebarAlertCounter className={S.alertCounter}>{props.itemCount}</SidebarAlertCounter>
          )}
    </div>
  );
}

function getSectionIconClass(sectionName: string, sidebarState: SidebarState) {
  return sidebarState.activeSection === sectionName
    ? S.sectionIcon  + " " + S.active
    : S.sectionIcon;
}