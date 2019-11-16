import React from "react";
import S from "./ScreenHeader.module.scss";

export class ScreenHeader extends React.Component {
  render() {
    return <div className={S.root}>{this.props.children}
      <div className={S.progressIndicator}>
        <div className={S.indefinite} />
      </div>
    </div>;
  }
}
