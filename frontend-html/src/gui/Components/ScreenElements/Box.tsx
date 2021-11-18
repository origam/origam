import React from "react";
import S from "./Box.module.css";

export class Box extends React.Component {
  render() {
    return <div className={S.box}>{this.props.children}</div>;
  }
}
