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
import { getUserNameFromToken } from "../../model/selectors/getUserNameFromToken";
import { getApi } from "../../model/selectors/getApi";
import { getApplicationLifecycle } from "../../model/selectors/getApplicationLifecycle";
import { IWorkbench } from "../../model/types/IWorkbench";
import { IMainMenu, ILoadingMainMenu } from "../../model/types/IMainMenu";

@inject(({ application }) => {
  return {
    workbench: application.workbench,
    mainMenu: application.workbench.mainMenu,
    loggedUserName: getUserNameFromToken(getApi(application).accessToken),
    onSignOutClick: (event: any) =>
      getApplicationLifecycle(application).onSignOutClick({ event }),
    onMainMenuItemClick: (event: any, item: any) =>
      getApplicationLifecycle(application).onMainMenuItemClick({ event, item })
  };
})
@observer
export class WorkbenchPage extends React.Component<{
  workbench?: IWorkbench;
  mainMenu?: IMainMenu | ILoadingMainMenu | undefined;
  loggedUserName?: string;
  onSignOutClick?: () => void;
  onMainMenuItemClick?: (event: any, item: any) => void;
}> {
  mainSplitterModel = new SplitterModel([["1", 1], ["2", 5]]);

  render() {
    return (
      <Provider workbench={this.props.workbench}>
        <div className={S.app}>
          <div className={S.headerBar}>
            <div className={S.logoSection}>
              <div className={S.logoBox}>
                <img className={S.logoImg} src="img/asap.png" />
              </div>
              <div className={S.searchBox}>
                <input />
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
            <div className={S.actionsSection}>
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
            </div>
            <div className={S.pusher} />
            <div className={S.companyUserSection}>
              <div className={S.companyLogo}>
                <img
                  className={S.companyLogoImg}
                  src="img/advantageSolutions.png"
                />
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
                <Observer>
                  {() =>
                    this.props.mainMenu && !this.props.mainMenu.isLoading ? (
                      <MainMenu
                        menuUI={(this.props.mainMenu as IMainMenu).menuUI}
                        onItemClick={this.props.onMainMenuItemClick}
                      />
                    ) : (
                      <></>
                    )
                  }
                </Observer>
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
