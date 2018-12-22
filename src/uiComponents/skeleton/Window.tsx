import * as React from "react";
import { observer, inject } from "mobx-react";
import { observable, action } from "mobx";

import { IOpenedView } from "src/Application/types";

interface IWindowProps {
  mainView?: IOpenedView;
  id: string;
  name: string;
}

@inject("mainView")
@observer
export class Window extends React.Component<IWindowProps> {
  @observable public isFullscreen = false;

  @action.bound
  public handleFullscreenClick(event: any) {
    this.isFullscreen = !this.isFullscreen;
  }

  public componentDidMount() {
    const mainView = this.props.mainView!;
    /* mainView.componentBindingsModel.start();
    mainView.unlockLoading(); */
  }

  public componentWillUnmount() {
    /* const mainView = this.props.mainView!;
    mainView.componentBindingsModel.stop(); */
  }

  public render() {
    const {name} = this.props;
    const {order, isActive} = this.props.mainView!;
    return (
      <div
        className={"oui-window" + (this.isFullscreen ? " fullscreen" : "")}
        style={{ display: !isActive ? "none" : undefined }}
      >
        <div className="oui-window-header">
          <div className="title">
            <i className="fa fa-file-text" />
            {name} {order > 0 && `[${order}]`}
          </div>
          <div style={{ flex: "1 1 0" }} />
          <div className="buttons">
            <button
              onClick={this.handleFullscreenClick}
              className={this.isFullscreen ? " active" : ""}
            >
              <i className="fa fa-external-link" />
            </button>
          </div>
        </div>
        <div className="oui-window-content">{this.props.children}</div>
      </div>
    );
  }
}
