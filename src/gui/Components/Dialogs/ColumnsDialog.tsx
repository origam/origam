import S from "./ColumnsDialog.module.css";
import React from "react";
import { ModalWindowOverlay, ModalWindow } from "../Dialog/Dialog";
import { AutoSizer, Grid } from "react-virtualized";
import { bind } from "bind-decorator";

export class ColumnsDialog extends React.Component {
  render() {
    return (
      <ModalWindowOverlay>
        <ModalWindow
          title="Columns"
          buttonsCenter={
            <>
              <button>OK</button>
              <button>Save As...</button>
              <button>Cancel</button>
            </>
          }
          buttonsLeft={null}
          buttonsRight={null}
        >
          <div className={S.columnTable}>
            <AutoSizer>
              {({ width, height }) => (
                <Grid
                  cellRenderer={this.renderCell}
                  columnCount={4}
                  rowCount={50}
                  columnWidth={({ index }: { index: number }) => {
                    switch (index) {
                      case 0:
                        return 30;
                      case 1:
                        return 300;
                      case 2:
                        return 45;
                      case 3:
                        return 75;
                      default:
                        return 100;
                    }
                  }}
                  rowHeight={20}
                  width={width}
                  height={height}
                />
              )}
            </AutoSizer>
          </div>
          <div className={S.lockedColumns}>
            Locked columns count
            <input className={S.lockedColumnsInput} type="text" />
          </div>
        </ModalWindow>
      </ModalWindowOverlay>
    );
  }

  @bind renderCell(args: {
    columnIndex: number;
    rowIndex: number;
    style: any;
    key: any;
  }): React.ReactNode {
    if (args.rowIndex > 0) {
      const rowClassName = args.rowIndex % 2 === 0 ? "even" : "odd";
      return (
        <div
          style={args.style}
          className={S.columnTableCell + " " + rowClassName}
        >
          {args.columnIndex};{args.rowIndex}
        </div>
      );
    } else {
      return (
        <div style={args.style} className={S.columnTableCell + " header"}>
          Header {args.columnIndex}
        </div>
      );
    }
  }
}
