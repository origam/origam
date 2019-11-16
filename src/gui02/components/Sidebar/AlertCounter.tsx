import React from "react";
import S from "./AlertCounter.module.scss";

export class SidebarAlertCounter extends React.Component {
  render() {
    return <div className={S.root}>{this.props.children}</div>;
  }
}
