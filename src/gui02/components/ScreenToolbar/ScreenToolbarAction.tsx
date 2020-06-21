import S from "./ScreenToolbarAction.module.scss";
import React from "react";
import cx from "classnames";

export class ScreenToolbarAction extends React.Component<{
  icon?: React.ReactNode;
  label?: React.ReactNode;
  isHidden?: boolean;
  rootRef?: any;
  onClick?(event: any): void;
}> {
  render() {
    return (
      <div
        ref={this.props.rootRef}
        className={cx(S.root, { isLabelless: !this.props.label, isHidden: this.props.isHidden })}
        onClick={this.props.onClick}
      >
        {this.props.icon && <div className={S.icon}>{this.props.icon}</div>}
        {this.props.label && <div className={S.label}>{this.props.label}</div>}
      </div>
    );
  }
}
