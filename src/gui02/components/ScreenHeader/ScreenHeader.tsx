import React from "react";
import S from "./ScreenHeader.module.scss";

export class ScreenHeader extends React.Component<{
  isLoading?: boolean;
}> {
  render() {
    return (
      <div className={S.root}>
        {this.props.children}
        {(this.props.isLoading || window.localStorage.getItem("debugKeepProgressIndicatorsOn")) && (
          <div className={S.progressIndicator}>
            <div className={S.indefinite} />
          </div>
        )}
      </div>
    );
  }
}
