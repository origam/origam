import React from "react";
import S from './ScreenHeaderRowNo.module.scss';

export class ScreenHeaderRowNo extends React.Component {
  render() {
    return (
      <div className={S.root}>{this.props.children}</div>
    )
  }
}