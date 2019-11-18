import S from "./ScreenToolbarAction.module.scss";
import React from "react";
import cx from "classnames";

export class ScreenToolbarAction extends React.Component<{
  icon: React.ReactNode;
  label?: React.ReactNode;
  onClick?(event: any): void;
}> {
  render() {
    return (
      <div
        className={cx(S.root, { isLabelless: !this.props.label })}
        onClick={this.props.onClick}
      >
        <div className={S.icon}>{this.props.icon}</div>
        {this.props.label && <div className={S.label}>{this.props.label}</div>}
      </div>
    );
  }
}
