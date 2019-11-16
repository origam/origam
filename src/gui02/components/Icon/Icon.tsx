import React from "react";
// import ReactSVG from "react-svg";
import Svg from "react-inlinesvg";
import S from "./Icon.module.scss";

export class Icon extends React.Component<{ src: string }> {
  render() {
    return <Svg src={this.props.src} className={`${S.root} icon`} />;
  }
}
