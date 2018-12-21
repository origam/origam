import * as React from "react";
import { observer, inject } from "mobx-react";
import { observable, action } from "mobx";

import { IOpenedView } from "src/Application/types";

@inject("mainView")
@observer
export class Window extends React.Component<any> {
  @observable public isFullscreen = false;

  @action.bound
  public handleFullscreenClick(event: any) {
    this.isFullscreen = !this.isFullscreen;
  }

  public componentDidMount() {
    const {mainView} = this.props as {mainView: IOpenedView};
    mainView.componentBindingsModel.start();
    mainView.unlockLoading();
  }

  public componentWillUnmount() {
    const {mainView} = this.props as {mainView: IOpenedView};
    mainView.componentBindingsModel.stop();
  }

  public render() {
    const {label, order} = this.props;
    return (
      <div
        className={"oui-window" + (this.isFullscreen ? " fullscreen" : "")}
        style={{ display: !this.props.active ? "none" : undefined }}
      >
        <div className="oui-window-header">
          <div className="title">
            <i className="fa fa-file-text" />
            {label} {order > 0 && `[${order}]`}
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
