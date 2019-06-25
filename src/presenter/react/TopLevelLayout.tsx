import { observer, Observer } from "mobx-react";
import React from "react";
import { observable } from "mobx";
import {
  SplitterModel,
  Splitter,
  SplitterPanel
} from "./controls/Splitter/Splitter";

import { ILoggedUser } from "../../LoggedUser/types/ILoggedUser";
import { IALogout } from "../../LoggedUser/types/IALogout";
import { MainMenu } from "./controls/MainMenu/MainMenu";
import { IMainMenu } from "../../MainMenu/types";
import { IMainViewsScope } from "../../factory/types/IMainViewsScope";
import { MainViews } from "./MainViews/MainViews";

@observer
export class TopLevelLayout extends React.Component<{
  loggedUser: ILoggedUser;
  aLogout: IALogout;
  mainMenu: IMainMenu;
  mainViewsScope: IMainViewsScope;
}> {
  mainSplitterModel = new SplitterModel([["1", 1], ["2", 5]]);

  public render() {
    const mainViewsScope = this.props.mainViewsScope!;
    const { loggedUser, aLogout, mainMenu } = this.props;
    return (
      <div className="oui-app">
        <div className="oui-header-bar">
          <div className="section logo-section">
            <div className="logo-box">
              <img className="logo-img" src="img/asap.png" />
            </div>
            <div className="search-box">
              <input />
              <div className="search-icon">
                <i className="fa fa-search icon" />
              </div>
            </div>
          </div>
          <div className="section view-setup-section">
            <div className="action-item">
              <i className="fa fa-list-ul icon" />
              Menu
            </div>
            <div className="action-item">
              <i className="far fa-life-ring icon" />
              Help
            </div>
            <div className="horizontal-items">
              <div className="action-item">
                <i className="fas fa-bars density-1 icon" />
              </div>
              <div className="action-item">
                <i className="fas fa-bars density-2 icon" />
              </div>
              <div className="action-item">
                <i className="fas fa-bars density-3 icon" />
              </div>
            </div>
          </div>
          <div className="section actions-section" id="form-actions-container">

          </div>
          <div className="pusher" />
          <div className="section company-user-section">
            <div className="company-logo">
              <img
                className="company-logo-img"
                src="img/advantageSolutions.png"
              />
            </div>
            <div className="logged-user">{loggedUser.userName}</div>
            <div className="logged-user-actions">
              <button className="btn-sign-out" onClick={aLogout.do}>
                Sign Out
              </button>
            </div>
          </div>
        </div>
        <div className="oui-body-bar">
          <Splitter
            handleSize={10}
            horizontal={true}
            name="main-splitter"
            model={this.mainSplitterModel}
          >
            <SplitterPanel id={"1"}>
              <Observer>{() => <MainMenu items={mainMenu.items} />}</Observer>
            </SplitterPanel>
            <SplitterPanel id={"2"}>
              <Observer>
                {() => (
                  <MainViews
                    mainViews={mainViewsScope.mainViews}
                    aOnCloseClick={mainViewsScope.aOnCloseClick}
                    aOnHandleClick={mainViewsScope.aOnHandleClick}
                  />
                )}
              </Observer>
            </SplitterPanel>
          </Splitter>
        </div>
      </div>
    );
  }
}
