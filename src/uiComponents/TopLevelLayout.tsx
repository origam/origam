import { Splitter, SplitterPanel } from './Splitter02';
import { inject, observer } from 'mobx-react';
import * as React from 'react';
import { MainMenuComponent } from 'src/MainMenu/MainMenuComponent';
import { MainTabsComponent } from 'src/MainTabs/MainTabsComponent';

@inject("mainViews")
@observer
export class TopLevelLayout extends React.Component {
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
          <Splitter handleSize={10} horizontal={true} name="MainSplitter">
            <SplitterPanel id={"1"}>
              <MainMenuComponent />
            </SplitterPanel>
            <SplitterPanel id={"2"}>
              <MainTabsComponent />
            </SplitterPanel>
          </Splitter>
          {/*<div className="oui-side-bar">
            <MainMenu mainViewEngine={this.mainViewEngine} />
          </div>
          <div className="oui-data-bar">
            <MainTabs mainViewEngine={this.mainViewEngine} />
    </div>*/}
        </div>
      </div>
    );
  }
}