import {Icon} from "gui/Components/Icon/Icon";
import {SidebarAlertCounter} from "gui/Components/Sidebar/AlertCounter";
import {LogoSection} from "gui/Components/Sidebar/LogoSection";
import {Sidebar} from "gui/Components/Sidebar/Sidebar";
import {SidebarSection} from "gui/Components/Sidebar/SidebarSection";
import {SidebarSectionDivider} from "gui/Components/Sidebar/SidebarSectionDivider";
import {SidebarSectionHeader} from "gui/Components/Sidebar/SidebarSectionHeader";
import React from "react";
import {CMainMenu} from "gui/connections/CMainMenu";
import {action, reaction} from "mobx";
import {SidebarSectionBody} from "gui/Components/Sidebar/SidebarSectionBody";
import {MobXProviderContext, observer} from "mobx-react";
import {getWorkQueuesTotalItemsCount} from "model/selectors/WorkQueues/getWorkQueuesTotalItemCount";
import {IWorkbench} from "model/entities/types/IWorkbench";
import {CWorkQueues} from "gui/connections/CWorkQueues";
import {IInfoSubsection} from "gui/connections/types";
import {CSidebarInfoSection} from "gui/connections/CSidebarInfoSection";
import {addRecordInfoExpandRequestHandler} from "model/actions-ui/RecordInfo/addRecordInfoExpandRequestHandler";
import {addRecordAuditExpandRequestHandler} from "model/actions-ui/RecordInfo/addRecordAuditExpandRequestHandler";
import {T} from "utils/translation";
import S from "gui/connections/CSidebar.module.scss";
import {getLogoUrl} from "model/selectors/getLogoUrl";
import {CChatSection} from "gui/connections/CChatSection";
import {getChatrooms} from "model/selectors/Chatrooms/getChatrooms";
import {getShowChat} from "model/selectors/PortalSettings/getShowChat";
import {getShowWorkQues} from "model/selectors/PortalSettings/getShowWorkQues";
import {getNotifications} from "model/selectors/Chatrooms/getNotifications";
import {SearchResults} from "gui/Components/Search/SearchResults";
import {CFavorites} from "gui/connections/CFavorites";
import {getFavorites} from "model/selectors/MainMenu/getFavorites";

@observer
export class CSidebar extends React.Component {
  static contextType = MobXProviderContext;

  get workbench(): IWorkbench {
    return this.context.workbench;
  }

  get sidebarState(){
    return this.workbench.sidebarState;
  }

  @action.bound handleExpandRecordAuditLog() {
    this.sidebarState.activeInfoSubsection = IInfoSubsection.Audit;
    this.sidebarState.activeSection = "Info";
  }

  @action.bound handleExpandRecordInfo() {
    this.sidebarState.activeInfoSubsection = IInfoSubsection.Info;
    this.sidebarState.activeSection = "Info";
  }

  disposers: any[] = [];

  componentDidMount() {
    this.disposers.push(
      addRecordInfoExpandRequestHandler(this.workbench)(this.handleExpandRecordInfo),
      addRecordAuditExpandRequestHandler(this.workbench)(this.handleExpandRecordAuditLog)
    );    
    this.disposers.push(
      reaction(
        () => getFavorites(this.workbench).favoriteFolders,
        favoriteFolders => {
          const firstNonEmpty = favoriteFolders.find(folder => folder.items.length > 0 && !folder.isPinned)
          if(firstNonEmpty){
            this.sidebarState.activeSection = firstNonEmpty.id;
          } 
      },
      {fireImmediately: true})
    );
  }

  componentWillUnmount() {
    this.disposers.forEach((disposer) => disposer());
  }

  renderWorkQuesSection() {
    const workQueuesItemsCount = getWorkQueuesTotalItemsCount(this.workbench);
    return (
      <SidebarSection isActive={this.sidebarState.activeSection === "WorkQueues"}>
        <SidebarSectionDivider/>
        <SidebarSectionHeader
          isActive={this.sidebarState.activeSection === "WorkQueues"}
          icon={
            <>
              <Icon src="./icons/work-queue.svg" tooltip={T("Work Queues", "work_queue_measure")}/>
              {workQueuesItemsCount > 0 && (
                <SidebarAlertCounter>{workQueuesItemsCount}</SidebarAlertCounter>
              )}
            </>
          }
          label={<>{T("Work Queues", "work_queue_measure")}</>}
          onClick={() => (this.sidebarState.activeSection = "WorkQueues")}
        />
        <SidebarSectionBody isActive={this.sidebarState.activeSection === "WorkQueues"}>
          <CWorkQueues/>
        </SidebarSectionBody>
      </SidebarSection>
    );
  }

  renderChatSection(): React.ReactNode {
    const totalUnreadMessages = getChatrooms(this.workbench).totalItemCount;
    return (
      <SidebarSection isActive={this.sidebarState.activeSection === "Chat"}>
        <SidebarSectionDivider/>
        <SidebarSectionHeader
          isActive={this.sidebarState.activeSection === "Chat"}
          icon={
            <>
              <Icon src="./icons/chat.svg" tooltip={T("Chat", "chat")}/>
              {totalUnreadMessages > 0 && (
                <SidebarAlertCounter>{totalUnreadMessages}</SidebarAlertCounter>
              )}
            </>
          }
          label={<>{T("Chat", "chat")}</>}
          onClick={() => (this.sidebarState.activeSection = "Chat")}
        />
        <SidebarSectionBody isActive={this.sidebarState.activeSection === "Chat"}>
          <CChatSection/>
        </SidebarSectionBody>
      </SidebarSection>
    );
  }

  render() {
    const showChat = getShowChat(this.workbench);
    const showWorkQues = getShowWorkQues(this.workbench);
    const notificationBox = getNotifications(this.workbench)?.notificationBox;
    const logoUrl = getLogoUrl(this.workbench);
    const favorites = getFavorites(this.workbench);

    return (
      <Sidebar>
        <LogoSection>
          <div className={S.logoLeft}>
            {notificationBox ? (
              <div dangerouslySetInnerHTML={{__html: notificationBox}}/>
            ) : (
              <img src={logoUrl}/>
            )}
          </div>
        </LogoSection>

        {favorites.favoriteFolders
          .filter((folder) => folder.isPinned)
          .map((folder) => (
            <CFavorites
              key={folder.id}
              ctx={this.workbench}
              folder={folder}
              isActive={true}
              forceOpen={true}
            />
          ))}

        {showWorkQues ? this.renderWorkQuesSection() : null}

        {showChat ? this.renderChatSection() : null}

        {favorites.favoriteFolders
          .filter((folder) => !folder.isPinned)
          .map((folder) => (
            <CFavorites
              key={folder.id}
              ctx={this.workbench}
              folder={folder}
              isActive={this.sidebarState.activeSection === folder.id}
              onHeaderClick={() => (this.sidebarState.activeSection = folder.id)}
            />
          ))}
        <SidebarSection isActive={this.sidebarState.activeSection === "Menu"}>
          <SidebarSectionDivider/>
          <SidebarSectionHeader
            isActive={this.sidebarState.activeSection === "Menu"}
            icon={<Icon src="./icons/menu.svg" tooltip={T("Menu", "menu")}/>}
            label={T("Menu", "menu")}
            onClick={() => (this.sidebarState.activeSection = "Menu")}
          />
          <SidebarSectionBody isActive={this.sidebarState.activeSection === "Menu"}>
            <CMainMenu/>
          </SidebarSectionBody>
        </SidebarSection>
        <SidebarSection isActive={this.sidebarState.activeSection === "Info"}>
          <SidebarSectionDivider/>
          <SidebarSectionHeader
            isActive={this.sidebarState.activeSection === "Info"}
            icon={<Icon src="./icons/info.svg" tooltip={T("Info", "infopanel_title")}/>}
            label={T("Info", "infopanel_title")}
            onClick={() => (this.sidebarState.activeSection = "Info")}
          />
          <SidebarSectionBody isActive={this.sidebarState.activeSection === "Info"}>
            <CSidebarInfoSection activeSubsection={this.sidebarState.activeInfoSubsection}/>
          </SidebarSectionBody>
        </SidebarSection>
        <SidebarSection isActive={this.sidebarState.activeSection === "Search"}>
          <SidebarSectionDivider/>
          <SidebarSectionHeader
            isActive={this.sidebarState.activeSection === "Search"}
            icon={
              <Icon
                src="./icons/search.svg"
                tooltip={T("Search", "search_result", this.sidebarState.resultCount)}
              />
            }
            label={T("Search", "search_result", this.sidebarState.resultCount)}
            onClick={() => (this.sidebarState.activeSection = "Search")}
          />
          <SidebarSectionBody isActive={this.sidebarState.activeSection === "Search"}>
            <SearchResults groups={this.sidebarState.searchResultGroups} ctx={this.workbench}/>
          </SidebarSectionBody>
          <SidebarSectionDivider/>
        </SidebarSection>
      </Sidebar>
    );
  }
}
