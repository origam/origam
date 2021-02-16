import React from "react";
import S from "gui/Components/MainMenu/MainMenuUL.module.scss";

export class MainMenuUL extends React.Component {
  render() {
    return <ul className={S.root}>{this.props.children}</ul>;
  }
}
