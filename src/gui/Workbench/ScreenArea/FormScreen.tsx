import S from "./FormScreen.module.css";
import React from "react";
import { observer } from "mobx-react";

@observer
export class FormScreen extends React.Component<{
  isLoading: boolean;
  isFullScreen: boolean;
  isVisible: boolean;
  title: string;
  isSessioned: boolean;
}> {
  render() {
    return (
      <div className={S.screen + (!this.props.isVisible ? " hidden" : "")}>
        <div className={S.screenHeader}>
          <div className={S.screenIcon}>
            {!this.props.isLoading ? (
              <i className="fas fa-file-alt" />
            ) : (
              <i className="fas fa-sync-alt fa-spin" />
            )}
          </div>
          {this.props.title} {this.props.isSessioned && <i> (sessioned)</i>}
          <div className={S.pusher} />
          <button
            className={
              S.btnFullScreen + (this.props.isFullScreen ? " active" : "")
            }
            onClick={undefined}
          >
            <i className="fas fa-external-link-alt" />
          </button>
        </div>
        <div className={S.screenBody}>{this.props.children}</div>
      </div>
    );
  }
}
