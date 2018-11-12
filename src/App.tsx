import * as React from "react";
import { observer } from "mobx-react";
import { MainMenu } from "./MainMenu/MainMenuComponent";
import { MainViewEngine } from "./MainTabs/MainViewEngine";
import { runInAction } from "mobx";
import { MainTabs } from "./MainTabs/MainTabs";

@observer
export default class App extends React.Component {
  constructor(props: any) {
    super(props);

    this.mainViewEngine = new MainViewEngine();
  }

  public componentDidMount() {
    runInAction(() => {
      this.mainViewEngine.start();
    });
  }

  public mainViewEngine: MainViewEngine;

  public render() {
    return (
      <div className="oui-app">
        <div className="oui-header-bar">
          <div className="section logo-section">
            <div className="logo-box">
              <span>
                <span className="t1">ORIGAM</span>{" "}
                <span className="t2">H5</span>
              </span>
            </div>
            <div className="search-box">
              <input />
              <div className="search-icon">
                <i className="fa fa-search" />
              </div>
            </div>
          </div>
          <div className="section view-setup-section">
            <div className="action-item">
              <i className="fa fa-list-ul" />
              Menu
            </div>
            <div className="action-item">
              <i className="fa fa-support" />
              Help
            </div>
            <div className="horizontal-items">
              <div className="action-item">
                <i className="fa fa-navicon density-1" />
              </div>
              <div className="action-item">
                <i className="fa fa-navicon density-2" />
              </div>
              <div className="action-item">
                <i className="fa fa-navicon density-3" />
              </div>
            </div>
          </div>
          <div className="section actions-section">
            <div className="action-item-big">
              <i className="fa fa-save" />
              <br />
              Save
            </div>
            <div className="action-item-big">
              <i className="fa fa-refresh" />
              <br />
              Reload
            </div>
          </div>
        </div>
        <div className="oui-body-bar">
          <div className="oui-side-bar">
            <MainMenu mainViewEngine={this.mainViewEngine} />
          </div>
          <div className="oui-data-bar">
            <MainTabs mainViewEngine={this.mainViewEngine} />
          </div>
        </div>
      </div>
    );
  }
}
