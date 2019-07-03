import React from "react";
import S from "./DataView.module.css";

export class DataView extends React.Component<{ height?: number }> {
  getDataViewStyle() {
    if (this.props.height !== undefined) {
      return {
        flexGrow: 0,
        height: this.props.height
      };
    } else {
      return {
        flexGrow: 1,
        width: "100%",
        height: "100%"
      };
    }
  }

  render() {
    return (
      <div className={S.dataView} style={this.getDataViewStyle()}>
        {this.props.children}
      </div>
    );
  }
}
