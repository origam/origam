import React from "react";
import S from "./TabbedViewHandle.module.scss";
import cx from "classnames";
import { Icon } from "../Icon/Icon";

export class TabbedViewHandle extends React.Component<{
  isActive?: boolean;
  hasCloseBtn?: boolean;
  onClick?(event: any): void;
  onCloseClick?(event: any): void;
}> {
  render() {
    return (
      <div className={cx(S.root, { isActive: this.props.isActive })}>
        <div className={S.label} onClick={this.props.onClick}>
          {this.props.children}
        </div>
        {this.props.hasCloseBtn && (
          <a className={S.closeBtn} onClick={this.props.onCloseClick}>
            <Icon src="./icons/close.svg" />
          </a>
        )}
      </div>
    );
  }
}
