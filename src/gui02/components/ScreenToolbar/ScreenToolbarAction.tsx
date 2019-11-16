import S from "./ScreenToolbarAction.module.scss";
import React from "react";

export class ScreenToolbarAction extends React.Component<{
  icon: React.ReactNode;
  label: React.ReactNode;
  onClick?(event: any): void;
}> {
  render() {
    return (
      <div className={S.root} onClick={this.props.onClick}>
        <div className={S.icon}>{this.props.icon}</div>
        <div className={S.label}>{this.props.label}</div>
      </div>
    );
  }
}
