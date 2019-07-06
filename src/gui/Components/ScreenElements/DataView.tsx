import React from "react";
import S from "./DataView.module.css";
import { Toolbar } from "./DataViewToolbar";
import { observer } from "mobx-react";

@observer
export class DataView extends React.Component<{
  height?: number;
  isHeadless: boolean;
}> {
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
        {!this.props.isHeadless && <Toolbar />}
        {this.props.children}
      </div>
    );
  }
}
