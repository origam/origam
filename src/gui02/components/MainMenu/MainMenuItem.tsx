import React from "react";

import cx from "classnames";

import S from "./MainMenuItem.module.scss";

export class MainMenuItem extends React.Component<{
  /** Content to render as an icon. */
  icon: React.ReactNode;
  /** Content to render as a label. */
  label: string;
  /** Level of nestedness for making tree structured menus. */
  level: number;
  /** Indicates that the item is currently active. */
  isActive: boolean;
  isHidden: boolean;
  isHighLighted?: boolean;

  isOpenedScreen?: boolean;
  isActiveScreen?: boolean;

  onClick?(event: any): void;
  onContextMenu?(event: any): void;
  refDom?: any;
}> {
  render() {
    return (
      <div className={S.linkContainer} onContextMenu={this.props.onContextMenu}>
        <a
          ref={this.props.refDom}
          className={cx(S.anchor, {
            isActive: this.props.isActive,
            isHidden: this.props.isHidden,
            isOpenedScreen: this.props.isOpenedScreen,
            isActiveScreen: this.props.isActiveScreen,
            isHighLighted: this.props.isHighLighted,
          })}
          style={{ paddingLeft: `${this.props.level * 1.6667}em` }}
          onClick={this.props.onClick}
          title={this.props.label}
        >
          <div className={S.icon}>{this.props.icon}</div>
          <div className={S.label}>{this.props.label}</div>
        </a>
      </div>
    );
  }
}
