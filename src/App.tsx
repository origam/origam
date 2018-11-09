import * as React from "react";
import { observer } from "mobx-react";
import { MainMenu } from "./MainMenu/MainMenuComponent";



export interface IMainTabHandleProps {
  label: string;
  active: boolean;
  onClick?: (event: any) => void;
  onCloseClick?: (event: any) => void;
}

const MainTabHandle = ({
  label,
  active,
  onClick,
  onCloseClick
}: IMainTabHandleProps) => {
  const handleCloseClick = (event: any) => {
    event.stopPropagation();
    onCloseClick && onCloseClick(event);
  };
  const handleClick = (event: any) => {
    onClick && onClick(event);
  };
  return (
    <div
      className={"oui-main-tab-handle" + (active ? " active" : "")}
      onClick={handleClick}
    >
      {label}
      <button className="handle-btn" onClick={handleCloseClick}>
        <i className="fa fa-close" />
      </button>
    </div>
  );
};

@observer
export default class App extends React.Component {
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
            <MainMenu />
          </div>
          <div className="oui-data-bar">
            <div className="oui-main-tabs">
              <div className="oui-main-tab-handles">
                <MainTabHandle label="This is tab 1 handle" active={false} />
                <MainTabHandle label="This is tab 2 handle" active={true} />
                <MainTabHandle label="This is tab 3 handle" active={false} />
              </div>
              <div className="oui-main-tab-contents">&nbsp;</div>
            </div>
          </div>
        </div>
      </div>
    );
  }
}
