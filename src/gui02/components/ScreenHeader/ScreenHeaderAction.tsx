import React from "react";
import S from "./ScreenHeaderAction.module.scss";
import cx from "classnames";

export class ScreenHeaderAction extends React.Component<{
  isActive?: boolean;
  className?: string;
}> {
  render() {
    return (
      <a
        className={cx(S.root, this.props.className, {
          isActive: this.props.isActive
        })}
      >
        {this.props.children}
      </a>
    );
  }
}
