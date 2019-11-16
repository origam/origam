import React from "react";
import S from "./ScreenTabbedViewHandleRow.module.scss";

export class ScreenTabbedViewHandleRow extends React.Component {
  render() {
    return <div className={S.root}>{this.props.children}</div>;
  }
}
