import * as React from "react";
import * as ReactDOM from "react-dom";
import { IPositionedFieldProps } from "./types";
import { observer } from "mobx-react";
import S from "./PositionedField.module.css";
import { currentRowCellsDimensions } from "./TableRendering/currentRowCells";
import { lastClickedCellRectangle } from "./TableRendering/cells/dataCell";

@observer
export class PositionedField extends React.Component<IPositionedFieldProps> {
  render() {
    const { columnIndex, worldBounds } = this.props;
    const columnLeft = lastClickedCellRectangle.columnLeft;
    const columnWidth = lastClickedCellRectangle.columnWidth;
    const rowTop = lastClickedCellRectangle.rowTop;
    const rowHeight = lastClickedCellRectangle.rowHeight;
    const { scrollTop, scrollLeft } = this.props.scrollOffsetSource;
    return ReactDOM.createPortal(
      <div
        className={S.positionedField}
        onClick={(event: any) => event.stopPropagation()}
        style={{
          top: worldBounds.top + rowTop - scrollTop,
          left:
            columnIndex < this.props.fixedColumnsCount
              ? worldBounds.left + columnLeft
              : worldBounds.left + columnLeft - scrollLeft,
          width: columnWidth,
          height: rowHeight
        }}
      >
        {this.props.children}
      </div>,
      document.getElementById("form-field-portal")!
    );
  }
}
