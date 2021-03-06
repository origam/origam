/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { observer, Observer } from "mobx-react";
import React, { useContext, useEffect, useMemo, createRef } from "react";
import { GridCellProps, MultiGrid } from "react-virtualized";
import { CtxCell } from "./Cells/CellsCommon";
import S from "@origam/components/src/components/Dropdown/Dropdown.module.scss"
import { CtxDropdownCtrlRect, CtxDropdownRefBody } from "@origam/components";
import { CtxDropdownEditor } from "./DropdownEditor";
import { rowHeight } from "gui/Components/ScreenElements/Table/TableRendering/cells/cellsCommon";
import cx from "classnames";
import { getCanvasFontSize, getTextWidth } from "utils/textMeasurement";
import { DropdownColumnDrivers, DropdownDataTable } from "modules/Editors/DropdownEditor/DropdownTableModel";
import { BoundingRect } from "react-measure";
import { IDropdownEditorBehavior } from "modules/Editors/DropdownEditor/DropdownEditorBehavior";
import { observable } from "mobx";

export function DropdownEditorBody() {
  const refCtxBody = useContext(CtxDropdownRefBody);
  const beh = useContext(CtxDropdownEditor).behavior;

  const ref = useMemo(
    () => (elm: any) => {
      refCtxBody(elm);
      beh.refDropdownBody(elm);
    },
    [beh, refCtxBody]
  );

  useEffect(() => {
    window.addEventListener("mousedown", beh.handleWindowMouseDown);
    return () => window.removeEventListener("mousedown", beh.handleWindowMouseDown);
  }, [beh]);

  const drivers = useContext(CtxDropdownEditor).setup.columnDrivers;
  const dataTable = useContext(CtxDropdownEditor).editorDataTable;
  const rectCtrl = useContext(CtxDropdownCtrlRect);

  return (
    <Observer>
      {() => (
        <div ref={ref} className={S.body} onMouseDown={beh.handleBodyMouseDown}>
          <DropdownEditorTable
            drivers={drivers}
            dataTable={dataTable}
            rectCtrl={rectCtrl}
            beh={beh}
            rowHeight={rowHeight}
          />
        </div>
      )}
    </Observer>
  );
}

@observer
export class DropdownEditorTable extends  React.Component<{
  drivers: DropdownColumnDrivers,
  dataTable: DropdownDataTable,
  rectCtrl: BoundingRect,
  beh: IDropdownEditorBehavior,
  rowHeight: number,
  height?: number
}> {
  refMultiGrid = createRef<MultiGrid>();
  @observable
  scrollbarSize = { horiz: 0, vert: 0 };
  hasHeader: boolean;
  hoveredRowIndex= - 1;
  columnCount = 0;
  readonly cellPadding = 20;
  readonly maxHeight = 150;

  get rowCount(){
    return this.props.dataTable.rowCount + (this.hasHeader ? 1 : 0);
  }

  get height(){
    if(this.props.height){
      return this.props.height;
    }
    let height = 0;
    for (let i = 0; i < this.rowCount; i++) {
      height = height + this.props.rowHeight;
    }
    return Math.min(height, this.maxHeight) + this.scrollbarSize.horiz;
  }

  constructor(props: any) {
    super(props);
    this.columnCount = this.props.drivers.driverCount;
    this.hasHeader = this.columnCount > 1;
  }

  handleScrollbarPresenceChange(args: {
    horizontal: boolean;
    size: number;
    vertical: boolean;
  }) {
    this.scrollbarSize = {
      horiz: args.horizontal ? args.size : 0,
      vert: args.vertical ? args.size : 0,
    };
  }


  renderTableCell({columnIndex, key, parent, rowIndex, style}: GridCellProps) {
    const Prov = CtxCell.Provider as any;
    return (
      <Prov
        key={key}
        value={{visibleColumnIndex: columnIndex, visibleRowIndex: rowIndex}}
        style={style}
      >
        {(this.hasHeader && rowIndex > 0) || !this.hasHeader ? (
          <div
            style={style}
            className={cx({ isHoveredRow: rowIndex === this.hoveredRowIndex })}
            onMouseOver={(evt) => {
              this.hoveredRowIndex = rowIndex;
            }}
            onMouseOut={(evt) => {
              this.hoveredRowIndex= -1;
            }}
          >
            <Observer>
              {() => (
                <>
                  {this.props.drivers
                    .getDriver(columnIndex)
                    .bodyCellDriver.render(rowIndex - (this.hasHeader ? 1 : 0))}
                </>
              )}
            </Observer>
          </div>
        ) : (
          <div style={style}>
            <Observer>
              {() => <>{this.props.drivers.getDriver(columnIndex).headerCellDriver.render()}</>}
            </Observer>
          </div>
        )}
      </Prov>
    );
  }

  render(){
    let columnWidthSum = 0;
    let width = 0;
    let widths: number[] = [];
    for (let columnIndex = 0; columnIndex < this.columnCount; columnIndex++) {
      let cellWidth = 100;
      for (let rowIndex = 0; rowIndex < this.rowCount - 1; rowIndex++) {
        const cellText = this.props.drivers.getDriver(columnIndex).bodyCellDriver.formattedText(rowIndex);
        const currentCellWidth = Math.round(getTextWidth(cellText, getCanvasFontSize())) + this.cellPadding;
        if(currentCellWidth > cellWidth){
          cellWidth = currentCellWidth;
        }
      }

      width = width + cellWidth;
      widths.push(cellWidth);
      columnWidthSum = columnWidthSum + cellWidth;
      if (width >= window.innerWidth - 100) {
        width = window.innerWidth - 100;
        break;
      }
    }

    width = Math.max(width + this.scrollbarSize.vert, this.props.rectCtrl.width!);
    let columnGrowFactor = 1;
    if (columnWidthSum > 0 && columnWidthSum < this.props.rectCtrl.width!) {
      columnGrowFactor = (width - this.scrollbarSize.vert) / columnWidthSum;
    }
    widths = widths.map((w) => w * columnGrowFactor);

    if(width === 0){
      return null;
    }

    return (
      <MultiGrid
        ref={this.refMultiGrid}
        scrollToRow={this.props.beh.scrollToRowIndex}
        scrollToAlignment="center"
        onScrollbarPresenceChange={args => this.handleScrollbarPresenceChange(args)}
        classNameTopRightGrid={S.table}
        classNameBottomRightGrid={S.table}
        columnCount={this.columnCount}
        rowCount={this.rowCount}
        columnWidth={({ index }) => widths[index]}
        rowHeight={this.props.rowHeight}
        fixedRowCount={this.hasHeader ? 1 : 0}
        height={this.height}
        width={width}
        cellRenderer={args => this.renderTableCell(args)}
        onScroll={this.props.beh.handleScroll}
      />
    );
  }
}
