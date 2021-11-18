import React from "react";
import S from "gui/Components/MainMenu/MainMenuLI.module.scss";

export class MainMenuLI extends React.Component {
  render() {
    return <li className={S.root}>{this.props.children}</li>;
  }
}
