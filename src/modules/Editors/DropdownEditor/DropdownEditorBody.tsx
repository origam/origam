import { Observer } from "mobx-react";
import React, { useContext, useEffect, useMemo, useState } from "react";
import { CellMeasurer, CellMeasurerCache, GridCellProps, MultiGrid } from "react-virtualized";
import { CtxCell } from "./Cells/CellsCommon";
import S from "./Dropdown/Dropdown.module.scss";
import { CtxDropdownCtrlRect, CtxDropdownRefBody } from "./Dropdown/DropdownCommon";
import { CtxDropdownEditor } from "./DropdownEditor";
import SE from "./DropdownEditor.module.scss";
import {rowHeight} from "gui/Components/ScreenElements/Table/TableRendering/cells/cellsCommon";

export function DropdownEditorBody() {
  const refCtxBody = useContext(CtxDropdownRefBody);
  const beh = useContext(CtxDropdownEditor).behavior;

  const ref = useMemo(() => (elm: any) => {
    refCtxBody(elm);
    beh.refDropdownBody(elm);
  }, [])

  useEffect(() => {
    window.addEventListener("mousedown", beh.handleWindowMouseDown);
    return () => window.removeEventListener("mousedown", beh.handleWindowMouseDown);
  }, []);
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

export function DropdownEditorTable() {
  
  const drivers = useContext(CtxDropdownEditor).columnDrivers;
  const dataTable = useContext(CtxDropdownEditor).editorDataTable;
  const beh = useContext(CtxDropdownEditor).behavior;
  const rectCtrl = useContext(CtxDropdownCtrlRect);
  
  const [cache] = useState(
    () =>
      new CellMeasurerCache({
        defaultHeight: 20,
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

  function renderTableCell({ columnIndex, key, parent, rowIndex, style }: GridCellProps) {
    return (
      <CtxCell.Provider
        key={key}
        value={{ visibleColumnIndex: columnIndex, visibleRowIndex: rowIndex }}
      >
        <CellMeasurer
          cache={cache}
          columnIndex={columnIndex}
          key={key}
          parent={parent}
          rowIndex={rowIndex}
        >
          {rowIndex ? (
            <div style={style}>
              <Observer>
                {() => <>{drivers.getDriver(columnIndex).bodyCellDriver.render(rowIndex - 1)}</>}
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
      </CtxCell.Provider>
    );
  }

  return (
    <Observer>
      {() => {
        const columnCount = drivers.driverCount;
        const rowCount = dataTable.rowCount + 1;

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

        return (
          <MultiGrid
            scrollToRow={beh.scrollToRowIndex}
            scrollToAlignment="center"
            onScrollbarPresenceChange={handleScrollbarPresenceChange}
            classNameTopRightGrid={SE.table}
            classNameBottomRightGrid={SE.table}
            columnCount={columnCount}
            rowCount={rowCount}
            columnWidth={({ index }) =>
              columnGrowFactor !== 1 ? widths[index] : cache.columnWidth({ index })
            }
            rowHeight={rowHeight}
            deferredMeasurementCache={cache}
            fixedRowCount={1}
            height={height}
            width={width}
            cellRenderer={renderTableCell}
            onScroll={beh.handleScroll}
          />
        );
      }}
    </Observer>
  );
}
