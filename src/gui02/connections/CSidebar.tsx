import {Icon} from "gui02/components/Icon/Icon";
import {SidebarAlertCounter} from "gui02/components/Sidebar/AlertCounter";
import {LogoSection} from "gui02/components/Sidebar/LogoSection";
import {Sidebar} from "gui02/components/Sidebar/Sidebar";
import {SidebarSection} from "gui02/components/Sidebar/SidebarSection";
import {SidebarSectionDivider} from "gui02/components/Sidebar/SidebarSectionDivider";
import {SidebarSectionHeader} from "gui02/components/Sidebar/SidebarSectionHeader";
import React from "react";
import {CMainMenu} from "./CMainMenu";
import {action, observable} from "mobx";
import {SidebarSectionBody} from "gui02/components/Sidebar/SidebarSectionBody";
import {MobXProviderContext, observer} from "mobx-react";
import {getWorkQueuesTotalItemsCount} from "model/selectors/WorkQueues/getWorkQueuesTotalItemCount";
import {IWorkbench} from "model/entities/types/IWorkbench";
import {CWorkQueues} from "./CWorkQueues";
import {IInfoSubsection} from "./types";
import {CSidebarInfoSection} from "./CSidebarInfoSection";
import {addRecordInfoExpandRequestHandler} from "model/actions-ui/RecordInfo/addRecordInfoExpandRequestHandler";
import {addRecordAuditExpandRequestHandler} from "model/actions-ui/RecordInfo/addRecordAuditExpandRequestHandler";
import {onSidebarInfoSectionCollapsed} from "model/actions-ui/RecordInfo/onSidebarInfoSectionCollapsed";
import {onSidebarAuditSectionExpanded} from "model/actions-ui/RecordInfo/onSidebarAuditSectionExpanded";
import {onSidebarInfoSectionExpanded} from "model/actions-ui/RecordInfo/onSidebarInfoSectionExpanded";
import {T} from "../../utils/translation";
import {getWorkbenchLifecycle} from "model/selectors/getWorkbenchLifecycle";
import {IWorkbenchLifecycle} from "model/entities/types/IWorkbenchLifecycle";
import S from "gui02/connections/CSidebar.module.scss";
import {getLogoUrl} from "model/selectors/getLogoUrl";
import {CChatSection} from "./CChatSection";
import {getChatrooms} from "model/selectors/Chatrooms/getChatrooms";
import {getShowChat} from "model/selectors/PortalSettings/getShowChat";
import {getShowWorkQues} from "model/selectors/PortalSettings/getShowWorkQues";
import {getNotifications} from "model/selectors/Chatrooms/getNotifications";
import {SearchBox} from "gui02/components/Search/SearchBox";
import {SearchResults} from "gui02/components/Search/SearchResults";
import {ISearchResult} from "model/entities/types/ISearchResult";
import {CFavorites} from "./CFavorites";
import {getFavorites} from "model/selectors/MainMenu/getFavorites";

@observer
export class CSidebar extends React.Component {
  static contextType = MobXProviderContext;

  private workbenchLifecycle: IWorkbenchLifecycle | undefined;

  get workbench(): IWorkbench {
    return this.context.workbench;
  }

  @observable searchResults: ISearchResult[] = [];
  @observable _activeSection = "Menu";
  @observable activeInfoSubsection = IInfoSubsection.Info;

  get activeSection() {
    return this._activeSection;
  }

  set activeSection(value) {
    if (this._activeSection === "Info" && value !== this._activeSection) {
      onSidebarInfoSectionCollapsed(this.workbench)();
    }
    if (this._activeSection !== "Info" && value === "Info") {
      if (this.activeInfoSubsection === IInfoSubsection.Info) {
        onSidebarInfoSectionExpanded(this.workbench)();
      }
      if (this.activeInfoSubsection === IInfoSubsection.Audit) {
        onSidebarAuditSectionExpanded(this.workbench)();
      }
    }
    this._activeSection = value;
  }

  @action.bound handleExpandRecordAuditLog() {
    this.activeInfoSubsection = IInfoSubsection.Audit;
    this.activeSection = "Info";
  }

  @action.bound handleExpandRecordInfo() {
    this.activeInfoSubsection = IInfoSubsection.Info;
    this.activeSection = "Info";
  }

  disposers: any[] = [];

  componentDidMount() {
    this.disposers.push(
      addRecordInfoExpandRequestHandler(this.workbench)(this.handleExpandRecordInfo),
      addRecordAuditExpandRequestHandler(this.workbench)(this.handleExpandRecordAuditLog)
    );
    this.workbenchLifecycle = getWorkbenchLifecycle(this.workbench);
  }

  componentWillUnmount() {
    this.disposers.forEach((disposer) => disposer());
  }

  @action
  onSearchResultsChange(results: ISearchResult[]) {
    this.searchResults = results;
    this.activeSection = "Search";
  }

  renderWorkQuesSection() {
    const workQueuesItemsCount = getWorkQueuesTotalItemsCount(this.workbench);
    return (
      <SidebarSection isActive={this.activeSection === "WorkQueues"}>
        <SidebarSectionDivider/>
        <SidebarSectionHeader
          isActive={this.activeSection === "WorkQueues"}
          icon={
            <>
              <Icon src="./icons/work-queue.svg" tooltip={T("Work Queues", "work_queue_measure")}/>
              {workQueuesItemsCount > 0 && (
                <SidebarAlertCounter>{workQueuesItemsCount}</SidebarAlertCounter>
              )}
            </>
          }
          label={<>{T("Work Queues", "work_queue_measure")}</>}
          onClick={() => (this.activeSection = "WorkQueues")}
        />
        <SidebarSectionBody isActive={this.activeSection === "WorkQueues"}>
          <CWorkQueues/>
        </SidebarSectionBody>
      </SidebarSection>
    );
  }

  renderChatSection(): React.ReactNode {
    const totalUnreadMessages = getChatrooms(this.workbench).totalItemCount;
    return (
      <SidebarSection isActive={this.activeSection === "Chat"}>
        <SidebarSectionDivider/>
        <SidebarSectionHeader
          isActive={this.activeSection === "Chat"}
          icon={
            <>
              <Icon src="./icons/chat.svg" tooltip={T("Chat", "chat")}/>
              {totalUnreadMessages > 0 && (
                <SidebarAlertCounter>{totalUnreadMessages}</SidebarAlertCounter>
              )}
            </>
          }
          label={<>{T("Chat", "chat")}</>}
          onClick={() => (this.activeSection = "Chat")}
        />
        <SidebarSectionBody isActive={this.activeSection === "Chat"}>
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
    const defaultFavoritesFolder = favorites.getFolder(favorites.dafaultFavoritesFolderId);
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

        <SearchBox
          ctx={this.workbench}
          onSearchResultsChange={(results) => this.onSearchResultsChange(results)}
        />

        {favorites.favoriteFolders
          .filter((folder) => folder.isPinned)
          .map((folder) => (
            <CFavorites
              ctx={this.workbench}
              folderId={folder.id}
              folderName={folder.name}
              isActive={true}
              forceOpen={true}
            />
          ))}

        {showWorkQues ? this.renderWorkQuesSection() : null}

        {showChat ? this.renderChatSection() : null}

        {defaultFavoritesFolder && !defaultFavoritesFolder?.isPinned && (
          <CFavorites
            ctx={this.workbench}
            folderId={defaultFavoritesFolder.id}
            folderName={T("Favorites", "default_group")}
            isActive={this.activeSection === defaultFavoritesFolder.id}
            onHeaderClick={() => (this.activeSection = defaultFavoritesFolder.id)}
          />
        )}
        {favorites.customFolders
          .filter((folder) => !folder.isPinned)
          .map((folder) => (
            <CFavorites
              ctx={this.workbench}
              folderId={folder.id}
              folderName={folder.name}
              isActive={this.activeSection === folder.id}
              onHeaderClick={() => (this.activeSection = folder.id)}
            />
          ))}
        <SidebarSection isActive={this.activeSection === "Menu"}>
          <SidebarSectionDivider/>
          <SidebarSectionHeader
            isActive={this.activeSection === "Menu"}
            icon={<Icon src="./icons/menu.svg" tooltip={T("Menu", "menu")}/>}
            label={T("Menu", "menu")}
            onClick={() => (this.activeSection = "Menu")}
          />
          <SidebarSectionBody isActive={this.activeSection === "Menu"}>
            <CMainMenu/>
          </SidebarSectionBody>
        </SidebarSection>
        <SidebarSection isActive={this.activeSection === "Info"}>
          <SidebarSectionDivider/>
          <SidebarSectionHeader
            isActive={this.activeSection === "Info"}
            icon={<Icon src="./icons/info.svg" tooltip={T("Info", "infopanel_title")}/>}
            label={T("Info", "infopanel_title")}
            onClick={() => (this.activeSection = "Info")}
          />
          <SidebarSectionBody isActive={this.activeSection === "Info"}>
            <CSidebarInfoSection activeSubsection={this.activeInfoSubsection}/>
          </SidebarSectionBody>
        </SidebarSection>
        <SidebarSection isActive={this.activeSection === "Search"}>
          <SidebarSectionDivider/>
          <SidebarSectionHeader
            isActive={this.activeSection === "Search"}
            icon={
              <Icon
                src="./icons/search.svg"
                tooltip={T("Search", "search_result", this.searchResults.length)}
              />
            }
            label={T("Search", "search_result", this.searchResults.length)}
            onClick={() => (this.activeSection = "Search")}
          />
          <SidebarSectionBody isActive={this.activeSection === "Search"}>
            <SearchResults results={this.searchResults}/>
          </SidebarSectionBody>
          <SidebarSectionDivider/>
        </SidebarSection>
      </Sidebar>
    );
  }
}
