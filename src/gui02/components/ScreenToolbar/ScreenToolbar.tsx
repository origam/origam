import React from "react";
import S from './ScreenToolbar.module.scss';


export class ScreenToolbar extends React.Component {
  render() {
    return <div className={S.root}>{this.props.children}</div>;
  }
}
