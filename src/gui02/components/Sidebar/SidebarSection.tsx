import React from "react";
import cx from "classnames";
import S from "./SidebarSection.module.scss";

export class SidebarSection extends React.Component<{ isActive: boolean }> {
  render() {
    return (
      <div
        className={cx(S.root, {
          isActive: this.props.isActive
        })}
      >
        {this.props.children}
      </div>
    );
  }
}
