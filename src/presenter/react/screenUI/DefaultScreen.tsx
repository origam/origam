import React from "react";
import { observer } from "mobx-react";
import { observable, action } from "mobx";

@observer
export class DefaultScreen extends React.Component<{
  title: string;
  isLoading: boolean;
  isVisible: boolean;
}> {
  @observable isFullScreen = false;

  @action.bound handleFullScreenBtnClick() {
    this.isFullScreen = !this.isFullScreen;
  }

  render() {
    return (
      <div
        className={
          "screen" +
          (this.isFullScreen ? " fullscreen" : "") +
          (this.props.isVisible ? "" : " hidden")
        }
      >
        <div className="screen-header">
          <div className="screen-icon">
            {!this.props.isLoading ? (
              <i className="fas fa-file-alt" />
            ) : (
              <i className="fas fa-sync-alt fa-spin" />
            )}
          </div>
          {this.props.title}
          <div className="pusher" />
          <button
            className={
              "screen-fullscreen" + (this.isFullScreen ? " active" : "")
            }
            onClick={this.handleFullScreenBtnClick}
          >
            <i className="fas fa-external-link-alt" />
          </button>
        </div>
        <div className="screen-body">{this.props.children}</div>
      </div>
    );
  }
}
