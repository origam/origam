import React from "react";

import cx from "classnames";

import S from "./MainMenuItem.module.scss";

export class MainMenuItem extends React.Component<{
  /** Content to render as an icon. */
  icon: React.ReactNode;
  /** Content to render as a label. */
  label: React.ReactNode;
  /** Level of nestedness for making tree structured menus. */
  level: number;
  /** Indicates that the item is currently active. */
  isActive: boolean;

  isHidden: boolean;

  onClick?(event: any): void;
}> {
  render() {
    return (
      <a
        className={cx(
          S.anchor,
          {
            isActive: this.props.isActive
          },
          { isHidden: this.props.isHidden }
        )}
        style={{ paddingLeft: `${this.props.level * 1.6667}em` }}
        onClick={this.props.onClick}
      >
        <div className={S.icon}>{this.props.icon}</div>
        <div className={S.label}>{this.props.label}</div>
      </a>
    );
  }
}
