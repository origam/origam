import React from "react";
import S from './HBox.module.scss';

export class HBox extends React.Component<{
  width?: number
}> {
  getHBoxStyle() {
    // TODO: Change to width?
    if (this.props.width !== undefined) {
      return {
        flexShrink: 0,
        width: this.props.width
      };
    } else {
      return {
        flexGrow: 1
      };
    }
  }

  render() {
    return (
      <div className={S.hBox} style={this.getHBoxStyle()}>
        {this.props.children}
      </div>
    );
  }
}