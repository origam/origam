import React from "react";
import S from './VBox.module.css';

export class VBox extends React.Component<{
  height?: number
}> {
  getVBoxStyle() {
    if (this.props.height !== undefined) {
      return {
        flexShrink: 0,
        height: this.props.height
      };
    } else {
      return {
        flexGrow: 1
      };
    }
  }

  render() {
    return (
      <div className={S.vBox} style={this.getVBoxStyle()}>
        {this.props.children}
      </div>
    );
  }
}