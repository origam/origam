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

import { Observer, observer } from "mobx-react";
import React, { useContext, useEffect, useMemo, useState, createRef } from "react";
import { CellMeasurer, CellMeasurerCache, GridCellProps, MultiGrid } from "react-virtualized";
import { CtxCell } from "./Cells/CellsCommon";
import S from "./Dropdown/Dropdown.module.scss";
import { CtxDropdownCtrlRect, CtxDropdownRefBody } from "./Dropdown/DropdownCommon";
import { CtxDropdownEditor } from "./DropdownEditor";
import SE from "./DropdownEditor.module.scss";
import { rowHeight } from "gui/Components/ScreenElements/Table/TableRendering/cells/cellsCommon";
import cx from "classnames";

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
  return (
    <Observer>
      {() => (
        <div ref={ref} className={S.body} onMouseDown={beh.handleBodyMouseDown}>
          <DropdownEditorTable />
        </div>
      )}
    </Observer>
  );
}

export const DropdownEditorTable = observer(function DropdownEditorTable() {
  const drivers = useContext(CtxDropdownEditor).columnDrivers;
  const dataTable = useContext(CtxDropdownEditor).editorDataTable;
  const beh = useContext(CtxDropdownEditor).behavior;
  const rectCtrl = useContext(CtxDropdownCtrlRect);
  const refMultiGrid = createRef<MultiGrid>();

  const [cache] = useState(
    () =>
      new CellMeasurerCache({
        defaultHeight: 25,
        defaultWidth: 100,
        fixedHeight: true,
        minWidth: 100,
      })
  );

  const [scrollbarSize, setScrollbarSize] = useState({ horiz: 0, vert: 0 });
  function handleScrollbarPresenceChange(args: {
    horizontal: boolean;
    size: number;
    vertical: boolean;
  }) {
    setScrollbarSize({
      horiz: args.horizontal ? args.size : 0,
      vert: args.vertical ? args.size : 0,
    });
  }

  const [hoveredRowIndex, setHoveredRowIndex] = useState(-1);

  function renderTableCell({ columnIndex, key, parent, rowIndex, style }: GridCellProps) {
    const Prov = CtxCell.Provider as any;
    return (
      <Prov
        key={key}
        value={{ visibleColumnIndex: columnIndex, visibleRowIndex: rowIndex }}
        style={style}
      >
        <CellMeasurer
          cache={cache}
          columnIndex={columnIndex}
          key={key}
          parent={parent}
          rowIndex={rowIndex}
        >
          {(hasHeader && rowIndex > 0) || !hasHeader ? (
            <div
              style={style}
              className={cx({ isHoveredRow: rowIndex === hoveredRowIndex })}
              onMouseOver={(evt) => {
                setHoveredRowIndex(rowIndex);
              }}
              onMouseOut={(evt) => {
                setHoveredRowIndex(-1);
              }}
            >
              <Observer>
                {() => (
                  <>
                    {drivers
                      .getDriver(columnIndex)
                      .bodyCellDriver.render(rowIndex - (hasHeader ? 1 : 0))}
                  </>
                )}
              </Observer>
            </div>
          ) : (
            <div style={style}>
              <Observer>
                {() => <>{drivers.getDriver(columnIndex).headerCellDriver.render()}</>}
              </Observer>
            </div>
          )}
        </CellMeasurer>
      </Prov>
    );
  }

  const columnCount = drivers.driverCount;
  const hasHeader = columnCount > 1;
  const rowCount = dataTable.rowCount + (hasHeader ? 1 : 0);

  let width = 0;
  let columnWidthSum = 0;
  let widths: number[] = [];
  for (let i = 0; i < columnCount; i++) {
    width = width + cache.columnWidth({ index: i });
    widths.push(cache.columnWidth({ index: i }));
    columnWidthSum = columnWidthSum + cache.columnWidth({ index: i });
    if (width >= window.innerWidth - 100) {
      width = window.innerWidth - 100;
      break;
    }
  }

  width = Math.max(width + scrollbarSize.vert, rectCtrl.width!);
  let columnGrowFactor = 1;
  if (columnWidthSum > 0 && columnWidthSum < rectCtrl.width!) {
    columnGrowFactor = (width - scrollbarSize.vert) / columnWidthSum;
  }
  widths = widths.map((w) => w * columnGrowFactor);

  let height = 0;
  for (let i = 0; i < rowCount; i++) {
    height = height + cache.rowHeight({ index: i });
  }
  height = Math.min(height, 300) + scrollbarSize.horiz;

  useEffect(() => {
    refMultiGrid.current?.recomputeGridSize();
  }, [width, refMultiGrid]);

  return (
    <MultiGrid
      ref={refMultiGrid}
      scrollToRow={beh.scrollToRowIndex}
      scrollToAlignment="center"
      onScrollbarPresenceChange={handleScrollbarPresenceChange}
      classNameTopRightGrid={SE.table}
      classNameBottomRightGrid={SE.table}
      columnCount={columnCount}
      rowCount={rowCount}
      columnWidth={({ index }) => {
        const cellWidth = columnGrowFactor !== 1 ? widths[index] : cache.columnWidth({ index });
        return cellWidth;
      }}
      rowHeight={rowHeight}
      deferredMeasurementCache={cache}
      fixedRowCount={hasHeader ? 1 : 0}
      height={height}
      width={width}
      cellRenderer={renderTableCell}
      onScroll={beh.handleScroll}
    />
  );
});
