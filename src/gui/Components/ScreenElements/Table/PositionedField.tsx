import * as React from "react";
import * as ReactDOM from "react-dom";
import {IPositionedFieldProps} from "./types";
import {observer} from "mobx-react";
import S from "./PositionedField.module.css";
import cx from "classnames";

@observer
export class PositionedField extends React.Component<IPositionedFieldProps> {
  render() {
    const { columnIndex,rowIndex, worldBounds, tablePanelView } = this.props;
    const cellRectangle = tablePanelView.getCellRectangle(rowIndex, columnIndex);
    const columnLeft = cellRectangle.columnLeft;
    const columnWidth = cellRectangle.columnWidth;
    const rowTop = cellRectangle.rowTop;
    const rowHeight = cellRectangle.rowHeight;
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
