import S from "./WorkbenchPage.module.css";
import React from "react";
import {
  SplitterPanel,
  Splitter,
  SplitterModel
} from "../Components/Splitter/Splitter";
import { MainMenu } from "./MainMenu/MainMenu";
import { ScreenArea } from "./ScreenArea/ScreenArea";
import { inject, observer, Provider, Observer } from "mobx-react";
import { getApplicationLifecycle } from "../../model/selectors/getApplicationLifecycle";
import { IWorkbench } from "../../model/entities/types/IWorkbench";
import { getWorkbench } from "../../model/selectors/getWorkbench";
import { getIsMainMenuLoading } from "../../model/selectors/MainMenu/getIsMainMenuLoading";
import { getLoggedUserName } from "../../model/selectors/User/getLoggedUserName";
import { getMainMenuUI } from "../../model/selectors/MainMenu/getMainMenuUI";
import { getMainMenuExists } from "../../model/selectors/MainMenu/getMainMenuExists";
import { getActiveScreenActions } from "model/selectors/getActiveScreenActions";
import { IAction } from "model/entities/types/IAction";
import {
  MainMenuPanelAccordion,
  MainMenuPanelAccordionHandle,
  MainMenuPanelAccordionBody
} from "./MainMenuPanelAccordion";
import { getClientFulltextSearch } from "model/selectors/getClientFulltextSearch";
import { ISearchResultSection } from "../../model/entities/types/IClientFulltextSearch";
import { SearchResultItem, SearchResultsPanel } from "./SearchResults";

@inject(({ application }) => {
  const clientFulltextSearch = getClientFulltextSearch(application);
  return {
    workbench: getWorkbench(application),
    isMainMenuLoading: getIsMainMenuLoading(application),
    mainMenuUI: getMainMenuUI(application),
    mainMenuExists: getMainMenuExists(application),
    loggedUserName: getLoggedUserName(application),
    toolbarActions: getActiveScreenActions(application),
    onSignOutClick: (event: any) =>
      getApplicationLifecycle(application).onSignOutClick({ event }),
    onMainMenuItemClick: (event: any, item: any) =>
      getApplicationLifecycle(application).onMainMenuItemClick({ event, item }),
    onSearchTermChange: clientFulltextSearch.onSearchFieldChange,
    subscribeOpenSearchSection: clientFulltextSearch.subscribeOpenSearchSection
  };
})
@observer
export class WorkbenchPage extends React.Component<{
  toolbarActions?: Array<{ section: string; actions: IAction[] }>;
  workbench?: IWorkbench;
  //mainMenu?: IMainMenu | ILoadingMainMenu | undefined;
  mainMenuUI?: any;
  mainMenuExists?: boolean;
  isMainMenuLoading?: boolean;
  loggedUserName?: string;
  onSignOutClick?: () => void;
  onMainMenuItemClick?: (event: any, item: any) => void;
  onSearchTermChange?: (event: any) => void;
  subscribeOpenSearchSection?(open: () => void): () => void;
}> {
  mainSplitterModel = new SplitterModel([["1", 1], ["2", 5]]);

  render() {
    return (
      <Provider workbench={this.props.workbench}>
        <div className={S.app}>
          <div className={S.headerBar}>
            <div className={S.logoSection}>
              <div className={S.logoBox}>
                <img className={S.logoImg} src="img/advantageSolutions.png" />
              </div>
              <div className={S.searchBox}>
                <input onChange={this.props.onSearchTermChange} />
                <div className={S.searchIcon}>
                  <i className="fa fa-search icon" />
                </div>
              </div>
            </div>
            <div className={S.viewSetupSection}>
              <div className={S.actionItemBig}>
                <i className="fa fa-list-ul icon" />
                Menu
              </div>
              <div className={S.actionItemBig}>
                <i className="far fa-life-ring icon" />
                Help
              </div>
              <div className={S.horizontalItems}>
                <div className={S.actionItemSmall}>
                  <i className="fas fa-bars density-1 icon" />
                </div>
                <div className={S.actionItemSmall}>
                  <i className="fas fa-bars density-2 icon" />
                </div>
                <div className={S.actionItemSmall}>
                  <i className="fas fa-bars density-3 icon" />
                </div>
              </div>
            </div>
            <ToolbarSection bottomLine="Form">
              <div className={S.actionItem}>
                <i className="far fa-save icon" />
                <br />
                Save
              </div>
              <div className={S.actionItem}>
                <i className="fas fa-redo icon" />
                <br />
                Reload
              </div>
            </ToolbarSection>
            {this.props.toolbarActions!.map(actionGroup => (
              <ToolbarSection bottomLine={actionGroup.section}>
                {actionGroup.actions.map(action => (
                  <div className={S.actionItem}>
                    <i className="fas fa-cog icon" />
                    <br />
                    {action.caption}
                  </div>
                ))}
              </ToolbarSection>
            ))}
            <div className={S.pusher} />
            <div className={S.companyUserSection}>
              <div className={S.companyLogo}>
                <img className={S.companyLogoImg} src="img/asap.png" />
              </div>
              <div className={S.loggedUser}>{this.props.loggedUserName}</div>
              <div className={S.loggedUserActions}>
                <button
                  className={S.btnSignOut}
                  onClick={this.props.onSignOutClick}
                >
                  Sign Out
                </button>
              </div>
            </div>
          </div>
          <div className={S.bodyBar}>
            <Splitter
              handleSize={10}
              horizontal={true}
              name="main-splitter"
              model={this.mainSplitterModel}
            >
              <SplitterPanel id={"1"}>
                <MainMenuPanelAccordion
                  subscribeActivator={setActiveSectionId =>
                    this.props.subscribeOpenSearchSection!(() =>
                      setActiveSectionId("search")
                    )
                  }
                >
                  <MainMenuPanelAccordionHandle id="workQueues">
                    <div className={S.accordionHandleIcon}>
                      <i className="far fa-envelope icon" />
                    </div>
                    Work Queues
                  </MainMenuPanelAccordionHandle>
                  <MainMenuPanelAccordionBody id="workQueues">
                    Work Queues
                  </MainMenuPanelAccordionBody>
                  <MainMenuPanelAccordionHandle id="favorites">
                    <div className={S.accordionHandleIcon}>
                      <i className="fas fa-star" />
                    </div>
                    Favourites
                  </MainMenuPanelAccordionHandle>
                  <MainMenuPanelAccordionBody id="favorites">
                    Favourites
                  </MainMenuPanelAccordionBody>
                  <MainMenuPanelAccordionHandle id="menu">
                    <div className={S.accordionHandleIcon}>
                      <i className="fas fa-home" />
                    </div>
                    Menu
                  </MainMenuPanelAccordionHandle>
                  <MainMenuPanelAccordionBody id="menu" initialActive={true}>
                    <Observer>
                      {() =>
                        this.props.mainMenuExists &&
                        !this.props.isMainMenuLoading ? (
                          <MainMenu
                            menuUI={this.props.mainMenuUI!}
                            onItemClick={this.props.onMainMenuItemClick}
                          />
                        ) : (
                          <></>
                        )
                      }
                    </Observer>
                  </MainMenuPanelAccordionBody>
                  <MainMenuPanelAccordionHandle id="info">
                    <div className={S.accordionHandleIcon}>
                      <i className="fas fa-info-circle" />
                    </div>
                    Info
                  </MainMenuPanelAccordionHandle>
                  <MainMenuPanelAccordionBody id="info">
                    Info
                  </MainMenuPanelAccordionBody>
                  <MainMenuPanelAccordionHandle id="search">
                    <div className={S.accordionHandleIcon}>
                      <i className="fas fa-search" />
                    </div>
                    Search
                  </MainMenuPanelAccordionHandle>
                  <MainMenuPanelAccordionBody id="search">
                    <SearchResultsPanel />
                  </MainMenuPanelAccordionBody>
                </MainMenuPanelAccordion>
                {/**/}
              </SplitterPanel>
              <SplitterPanel id={"2"}>
                <ScreenArea />
              </SplitterPanel>
            </Splitter>
          </div>
        </div>
      </Provider>
    );
  }
}

export const ToolbarSection: React.FC<{
  bottomLine: React.ReactNode;
}> = props => (
  <div className={S.actionsSection}>
    <div className={S.actionsSectionActions}>{props.children}</div>
    <div className={S.actionsSectionBottomLine}>{props.bottomLine}</div>
  </div>
);
