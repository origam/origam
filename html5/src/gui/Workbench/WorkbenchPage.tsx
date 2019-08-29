import S from "./WorkbenchPage.module.css";
import React, { useContext } from "react";
import {
  SplitterPanel,
  Splitter,
  SplitterModel
} from "../Components/Splitter/Splitter";
import { MainMenu } from "./MainMenu/MainMenu";
import { ScreenArea } from "./ScreenArea/ScreenArea";
import {
  inject,
  observer,
  Provider,
  Observer,
  MobXProviderContext
} from "mobx-react";
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
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";
import { MainMenuPanel } from "./MainMenu/MainMenuPanel";
import { IApplication } from "model/entities/types/IApplication";
import { getActivePanelView } from "model/selectors/DataView/getActivePanelView";
import { getActiveScreen } from "../../model/selectors/getActiveScreen";
import { isILoadedFormScreen } from "../../model/entities/types/IFormScreen";
import { onSaveSessionClick } from "../../model/actions/onSaveSessionClick";
import { onRefreshSessionClick } from "model/actions/onRefreshSessionClick";
import { onActionClick } from "../../model/actions/Actions/onActionClick";

@inject(({ application }) => {
  const clientFulltextSearch = getClientFulltextSearch(application);
  return {
    workbench: getWorkbench(application),
    isMainMenuLoading: getIsMainMenuLoading(application),
    loggedUserName: getLoggedUserName(application),
    onSignOutClick: (event: any) =>
      getApplicationLifecycle(application).onSignOutClick({ event }),
    onSearchTermChange: clientFulltextSearch.onSearchFieldChange,
    subscribeOpenSearchSection: clientFulltextSearch.subscribeOpenSearchSection
  };
})
@observer
export class WorkbenchPage extends React.Component<{
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
            <FormButtonsSection />
            <ActionsSection />
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
                    <MainMenuPanel />
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

export const FormButtonsSection: React.FC<{}> = observer(props => {
  const application = useContext(MobXProviderContext)
    .application as IApplication;
  const activeScreen = getActiveScreen(application);
  const formScreen =
    activeScreen && isILoadedFormScreen(activeScreen.content)
      ? activeScreen.content
      : undefined;
  const isDirty = formScreen && formScreen.isDirty;
  return formScreen ? (
    <ToolbarSection bottomLine="Form">
      {isDirty && (
        <div className={S.actionItem} onClick={onSaveSessionClick(formScreen)}>
          <i className="far fa-save icon" />
          <br />
          Save
        </div>
      )}
      <div className={S.actionItem} onClick={onRefreshSessionClick(formScreen)}>
        <i className="fas fa-redo icon" />
        <br />
        Reload
      </div>
    </ToolbarSection>
  ) : null;
});

export const ActionsSection: React.FC<{}> = observer(props => {
  const application = useContext(MobXProviderContext)
    .application as IApplication;
  const toolbarActions = getActiveScreenActions(application);
  return (
    <>
      {toolbarActions.map(actionGroup => (
        <ToolbarSection bottomLine={actionGroup.section}>
          {actionGroup.actions.map(action => (
            <div
              className={S.actionItem}
              onClick={event => onActionClick(action)(event, action)}
            >
              <i className="fas fa-cog icon" />
              <br />
              {action.caption}
            </div>
          ))}
        </ToolbarSection>
      ))}
    </>
  );
});
