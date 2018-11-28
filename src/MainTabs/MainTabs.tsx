import * as React from "react";
import { observer, inject, Provider } from "mobx-react";
import { MainViewEngine } from "./MainViewEngine";
import { computed } from "mobx";

interface IMainTabsProps {
  mainViewEngine: MainViewEngine;
}

@observer
export class MainTabs extends React.Component<IMainTabsProps> {
  public render() {
    const { mainViewEngine } = this.props;

    return (
      <div className="oui-main-tabs">
        <div className="oui-main-tab-handles">
          {mainViewEngine.openedViews.map(view => {
            return (
              <MainTabHandleCnd
                key={`${view.view.id}@${view.view.subid}`}
                id={view.view.id}
                subid={view.view.subid}
                label={view.view.label}
                order={view.order}
                mainViewEngine={mainViewEngine}
              />
            );
          })}
        </div>
        <div className="oui-main-tab-contents">
          {mainViewEngine.openedViews.map(view => {
            return (
              <MainView
                key={`${view.view.id}@${view.view.subid}`}
                id={view.view.id}
                subid={view.view.subid}
                label={view.view.label}
                order={view.order}
                mainViewEngine={mainViewEngine}
                view={view}
              />
            );
          })}
        </div>
      </div>
    );
  }
}

@observer
export class MainView extends React.Component<any> {
  @computed
  public get isActive() {
    return (
      this.props.mainViewEngine &&
      this.props.mainViewEngine.activeView &&
      this.props.id === this.props.mainViewEngine.activeView.id &&
      this.props.subid === this.props.mainViewEngine.activeView.subid
    );
  }

  public render() {
    return (
      <Provider mainView={this.props.view.view}>
        {React.cloneElement(this.props.view.view.reactTree || <></>, {
          active: this.isActive,
          ...this.props
        })}
      </Provider>
    );
  }
}

@observer
export class MainTabHandleCnd extends React.Component<any> {
  @computed
  public get isActive() {
    return (
      this.props.mainViewEngine &&
      this.props.mainViewEngine.activeView &&
      this.props.id === this.props.mainViewEngine.activeView.id &&
      this.props.subid === this.props.mainViewEngine.activeView.subid
    );
  }

  public render() {
    return (
      <MainTabHandle
        {...this.props as any}
        active={this.isActive}
        onCloseClick={() =>
          this.props.mainViewEngine.closeView(this.props.id, this.props.subid)
        }
        onClick={() =>
          this.props.mainViewEngine.activateView(
            this.props.id,
            this.props.subid
          )
        }
      />
    );
  }
}

export interface IMainTabHandleProps {
  label: string;
  order: number;
  active: boolean;
  onClick?: (event: any) => void;
  onCloseClick?: (event: any) => void;
}

const MainTabHandle = ({
  label,
  order,
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
      {label} {order > 0 && `[${order}]`}
      <button className="handle-btn" onClick={handleCloseClick}>
        <i className="fa fa-close" />
      </button>
    </div>
  );
};
