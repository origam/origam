import React from "react";
import S from "./LogoSection.module.scss";

export class LogoSection extends React.Component {
  render() {
    return <div className={S.root}>{this.props.children}</div>;
  }
}
