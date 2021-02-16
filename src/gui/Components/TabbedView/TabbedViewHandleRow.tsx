import React from "react";
import S from "gui/Components/TabbedView/TabbedViewHandleRow.module.scss";

export class TabbedViewHandleRow extends React.Component {
  render() {
    return <div className={S.root}>{this.props.children}</div>;
  }
}
