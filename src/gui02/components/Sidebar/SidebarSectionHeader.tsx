import React from "react";
import cx from "classnames";
import S from "./SidebarSectionHeader.module.scss";

export class SidebarSectionHeader extends React.Component<{
  icon: React.ReactNode;
  label: React.ReactNode;
  isActive: boolean;
  onClick?(event: any): void;
}> {
  render() {
    return (
      <a
        className={cx(S.root, {
          isActive: this.props.isActive
        })}
        onClick={this.props.onClick}
      >
        <div className={S.icon}>{this.props.icon}</div>
        <div className={S.label}>{this.props.label}</div>
      </a>
    );
  }
}
