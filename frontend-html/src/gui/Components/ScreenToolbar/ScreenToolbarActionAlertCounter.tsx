import React from "react";
import S from "gui/Components/ScreenToolbar/ScreenToolbarActionAlertCounter.module.scss";

export class ScreenToolbarAlertCounter extends React.Component {
  render() {
    return <div className={S.root}>{this.props.children}</div>;
  }
}
