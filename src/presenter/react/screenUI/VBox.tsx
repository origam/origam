import * as React from "react";
import { observer } from "mobx-react";

import style from './ScreenElements.module.css';
import { parseNumber } from "../../../utils/xml";

@observer
export class VBox extends React.Component<{ Height: string | undefined }> {
  getVBoxStyle() {
    if (this.props.Height !== undefined) {
      return {
        flexShrink: 0,
        height: parseNumber(this.props.Height)
      };
    } else {
      return {
        flexGrow: 1
      };
    }
  }

  render() {
    return (
      <div className={style.VBox} style={this.getVBoxStyle()}>
        {this.props.children}
      </div>
    );
  }
}