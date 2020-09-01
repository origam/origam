import React from "react";
// import ReactSVG from "react-svg";
import Svg from "react-inlinesvg";
import S from "./Icon.module.scss";
import cx from "classnames";

export class Icon extends React.Component<{ src: string; className?: string; tooltip?: string }> {
  render() {
    return (
      <Svg
        title={this.props.tooltip}
        src={this.props.src}
        className={cx(S.root, "icon", this.props.className)}
      />
    );
  }
}
