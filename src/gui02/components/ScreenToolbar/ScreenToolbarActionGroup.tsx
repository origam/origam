import React from "react";
import S from './ScreenToolbarActionGroup.module.scss';

export class ScreenToolbarActionGroup extends React.Component {
  render() {
    return <div className={S.root}>{this.props.children}</div>;
  }
}
