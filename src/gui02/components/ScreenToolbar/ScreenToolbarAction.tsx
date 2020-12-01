import S from "./ScreenToolbarAction.module.scss";
import React from "react";
import cx from "classnames";

export class ScreenToolbarAction extends React.Component<{
  icon?: React.ReactNode;
  label?: string;
  isHidden?: boolean;
  rootRef?: any;
  onMouseDown?(event: any): void;
  onClick?(event: any): void;
}> {
  render() {
    return (
      <div
        ref={this.props.rootRef}
        className={cx(S.root, { isLabelless: !this.props.label, isHidden: this.props.isHidden })}
        onMouseDown={this.props.onMouseDown}
        onClick={this.props.onClick}
        title={this.props.label}
      >
        {this.props.icon && <div className={S.icon}>{this.props.icon}</div>}
        {this.props.label && <div className={S.label}>{this.props.label}</div>}
      </div>
    );
  }
}
