import * as React from "react";
import { IHeaderRowProps } from "./types";
import { observer } from "mobx-react";
import S from "./HeaderRow.module.css";

@observer
export class HeaderRow extends React.Component<IHeaderRowProps> {
  render() {
    const headers = [];
    for (
      let i = this.props.columnStartIndex;
      i < this.props.columnEndIndex;
      i++
    ) {
      headers.push(
        this.props.renderHeader({
          columnIndex: i,
          columnWidth: this.props.gridDimensions.getColumnWidth(i)
        })
      );
    }
    return <div className={S.headerRow} style={{zIndex: this.props.zIndex}}>{headers}</div>;
  }
}
