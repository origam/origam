import { computed } from "mobx";
import { observer, Provider, inject } from "mobx-react";
import * as React from "react";
import { IMainViews, IOpenedView } from "../Application/types";


interface IMainTabsProps {
  mainViews?: IMainViews;
}

@inject("mainViews")
@observer
export class MainTabsComponent extends React.Component<IMainTabsProps> {
  public render() {
    const mainViews = this.props.mainViews!;

    return (
      <div className="oui-main-tabs">
        <div className="oui-main-tab-handles">
          {mainViews.openedViews.map(view => {
            return (
              <MainTabHandleCnd
                key={`${view.id}@${view.subid}`}
                id={view.id}
                subid={view.subid}
                label={view.label}
                order={view.order}
                view={view}
              />
            );
          })}
        </div>
        <div className="oui-main-tab-contents">
          {mainViews.openedViews.map(view => {
            return (
              <MainViewComponent
                key={`${view.id}@${view.subid}`}
                id={view.id}
                subid={view.subid}
                label={view.label}
                order={view.order}
                view={view}
              />
            );
          })}
        </div>
      </div>
    );
  }
}

interface IMainViewComponentProps {
  mainViews?: IMainViews;
  view?: IOpenedView;
  id: string;
  subid: string;
  label: string;
  order: number;
}

@observer
export class MainViewComponent extends React.Component<
  IMainViewComponentProps
> {
  public render() {
    console.log('R', this.props.view!.reactTree)
    return (
      <Provider mainView={this.props.view}>
        {this.props.view!.reactTree || <></>}
      </Provider>
    );
  }
}

interface IMainTabHandleCnd {
  view: IOpenedView;
  mainViews?: IMainViews;
  id: string;
  subid: string;
  label: string;
  order: number;
}

@inject("mainViews")
@observer
export class MainTabHandleCnd extends React.Component<IMainTabHandleCnd> {
  @computed
  public get isActive() {
    return this.props.view.isActive;
  }

  public render() {
    return (
      <MainTabHandle
        {...this.props as any}
        active={this.isActive}
        onCloseClick={() =>
          this.props.mainViews!.closeView(this.props.id, this.props.subid)
        }
        onClick={() =>
          this.props.mainViews!.activateView(
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
