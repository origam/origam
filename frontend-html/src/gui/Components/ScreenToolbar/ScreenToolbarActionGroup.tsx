import React from "react";
import S from "gui/Components/ScreenToolbar/ScreenToolbarActionGroup.module.scss";
import cx from "classnames";

export class ScreenToolbarActionGroup extends React.Component<{
  domRef?: any;
  grovable?: boolean;
}> {
  render() {
    return (
      <div ref={this.props.domRef} className={cx(S.root, { grovable: this.props.grovable })}>
        {this.props.children}
      </div>
    );
  }
}
