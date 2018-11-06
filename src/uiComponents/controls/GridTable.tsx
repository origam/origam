import { observer, Observer, inject } from "mobx-react";
import * as React from "react";
import { IGridPanelBacking } from "src/GridPanel/types";
import { ColumnHeaders, GridComponent } from "src/Grid/GridComponent";
import { createColumnHeaderRenderer } from "src/Grid/ColumnHeaderRenderer";
import { IGridPaneView } from "src/Grid/types";
import { AutoSizer } from "react-virtualized";
import { GridCursorComponent } from "src/Grid/GridCursorComponent";
import { GridEditorMounter } from "src/cells/GridEditorMounter";
import { StringGridEditor } from "src/cells/string/GridEditor";
import { createGridCellRenderer } from "src/Grid/GridCellRenderer";

@inject("gridPaneBacking")
@observer
export class GridTable extends React.Component<any> {
  public render() {
    const {
      gridToolbarView,
      gridView,
      gridSetup,
      gridTopology,
      gridCursorView,
      gridInteractionActions,
      gridInteractionSelectors,
      formView,
      formSetup,
      formTopology,
      formActions
    } = this.props.gridPaneBacking;

    const isActiveView =
      gridInteractionSelectors.activeView === IGridPaneView.Grid;
    return (
      <div
        style={{
          display: !isActiveView ? "none" : "flex",
          flexDirection: "column",
          flex: "1 1"
        }}
      >
        <div
          style={{
            display: "flex",
            flexDirection: "column"
          }}
        >
          <ColumnHeaders
            view={gridView}
            columnHeaderRenderer={createColumnHeaderRenderer({
              gridSetup
            })}
          />
        </div>

        <div
          style={{
            flexDirection: "column",
            flex: "1 1",
            display:
              gridInteractionSelectors.activeView === IGridPaneView.Grid
                ? "flex"
                : "none"
          }}
        >
          <AutoSizer>
            {({ width, height }) => (
              <Observer>
                {() => (
                  <GridComponent
                    view={gridView}
                    gridSetup={gridSetup}
                    gridTopology={gridTopology}
                    width={width}
                    height={height}
                    overlayElements={
                      <GridCursorComponent
                        view={gridCursorView}
                        cursorContent={
                          gridInteractionSelectors.activeView ===
                            IGridPaneView.Grid && (
                            <GridEditorMounter cursorView={gridCursorView}>
                              {gridCursorView.isCellEditing && (
                                <StringGridEditor
                                  editingRecordId={gridCursorView.editingRowId!}
                                  editingFieldId={
                                    gridCursorView.editingColumnId!
                                  }
                                  value={
                                    gridCursorView.editingOriginalCellValue
                                  }
                                  onKeyDown={
                                    gridInteractionActions.handleDumbEditorKeyDown
                                  }
                                  onDataCommit={gridCursorView.handleDataCommit}
                                />
                              )}
                            </GridEditorMounter>
                          )
                        }
                      />
                    }
                    cellRenderer={createGridCellRenderer({
                      gridSetup,
                      onClick(event, cellRect, cellInfo) {
                        gridInteractionActions.handleGridCellClick(event, {
                          rowId: gridTopology.getRowIdByIndex(
                            cellInfo.rowIndex
                          )!,
                          columnId: gridTopology.getColumnIdByIndex(
                            cellInfo.columnIndex
                          )!
                        });
                      }
                    })}
                    onKeyDown={gridInteractionActions.handleGridKeyDown}
                    onOutsideClick={
                      gridInteractionActions.handleGridOutsideClick
                    }
                    onNoCellClick={gridInteractionActions.handleGridNoCellClick}
                  />
                )}
              </Observer>
            )}
          </AutoSizer>
        </div>
      </div>
    );
  }
}
