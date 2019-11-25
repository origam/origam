import React from "react";
import S from "./ScreenHeaderAction.module.scss";
import cx from "classnames";

export class ScreenHeaderAction extends React.Component<{
  isActive?: boolean;
  className?: string;
  onClick?(event: any): void;
}> {
  render() {
    return (
      <a
        className={cx(S.root, this.props.className, {
          isActive: this.props.isActive
        })}
        onClick={this.props.onClick}
      >
        {this.props.children}
      </a>
    );
  }
}
