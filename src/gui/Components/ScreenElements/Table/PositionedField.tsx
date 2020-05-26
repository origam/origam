import * as React from "react";
import * as ReactDOM from "react-dom";
import { IPositionedFieldProps } from "./types";
import { observer } from "mobx-react";
import S from "./PositionedField.module.css";
import cx from "classnames";

@observer
export class PositionedField extends React.Component<IPositionedFieldProps> {
  render() {
    const dim = this.props.gridDimensions;
    const { columnIndex, rowIndex, worldBounds } = this.props;
    const columnLeft = dim.getColumnLeft(columnIndex);
    const columnWidth = dim.getColumnWidth(columnIndex);
    const rowTop = dim.getRowTop(rowIndex);
    const rowHeight = dim.getRowHeight(rowIndex);
    const { scrollTop, scrollLeft } = this.props.scrollOffsetSource;
    return ReactDOM.createPortal(
      <div
        className={cx(S.positionedField, { isFirstColumn: columnIndex === 0 })}
        onClick={(event: any) => event.stopPropagation()}
        style={{
          top: worldBounds.top + rowTop - scrollTop,
          left:
            columnIndex < this.props.fixedColumnsCount
              ? worldBounds.left + columnLeft
              : worldBounds.left + columnLeft - scrollLeft,
          width: columnWidth,
          height: rowHeight,
        }}
      >
        {this.props.children}
      </div>,
      document.getElementById("form-field-portal")!
    );
  }
}
