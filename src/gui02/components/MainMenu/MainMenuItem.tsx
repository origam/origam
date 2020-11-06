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

  isOpenedScreen?: boolean;
  isActiveScreen?: boolean;

  onClick?(event: any): void;
  onContextMenu?(event: any): void;
  refDom?: any;
}> {
  render() {
    return (
      <a
        ref={this.props.refDom}
        className={cx(S.anchor, {
          isActive: this.props.isActive,
          isHidden: this.props.isHidden,
          isOpenedScreen: this.props.isOpenedScreen,
          isActiveScreen: this.props.isActiveScreen,
        })}
        style={{paddingLeft: `${this.props.level * 1.6667}em`}}
        onClick={this.props.onClick}
        onContextMenu={this.props.onContextMenu}
      >
        <div className={S.icon}>{this.props.icon}</div>
        <div className={S.label}>{this.props.label}</div>
      </a>
    );
  }
}
