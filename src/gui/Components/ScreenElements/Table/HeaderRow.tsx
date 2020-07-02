import * as React from "react";
import {IHeaderRowProps} from "./types";
import {observer} from "mobx-react";
import S from "./HeaderRow.module.css";

@observer
export class HeaderRow extends React.Component<IHeaderRowProps> {
  render() {
    return <div className={S.headerRow} style={{zIndex: this.props.zIndex || 0}}>{this.props.headerElements}</div>;
  }
}
