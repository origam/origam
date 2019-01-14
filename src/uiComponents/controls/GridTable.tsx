import { observer, Observer, inject } from "mobx-react";
import * as React from "react";
import { IGridPanelBacking } from "src/GridPanel/types";
import { ColumnHeaders, GridComponent } from "src/Grid/GridComponent";

import { GridViewType } from "src/Grid/types";
import { AutoSizer } from "react-virtualized";
import { GridCursorComponent } from "src/Grid/GridCursorComponent";
import { GridEditorMounter } from "src/cells/GridEditorMounter";
import { StringGridEditor } from "src/cells/string/GridEditor";
import { createGridCellRenderer } from "src/Grid/GridCellRenderer";

import Measure from "react-measure";
import { action, observable } from "mobx";
import * as _ from "lodash";
import { IDataTableRecord } from "src/DataTable/types";
import { IDataTableFieldStruct } from "../../DataTable/types";
import { ComboGridEditor } from "../../cells/stringCombo/GridEditor";

function getEditorClass(
  record: IDataTableRecord,
  field: IDataTableFieldStruct
): React.ComponentClass<any> {
  if (field.isLookedUp) {
    return ComboGridEditor;
  } else {
    return StringGridEditor;
  }
}

@inject("gridPaneBacking")
@observer
export class GridTable extends React.Component<any> {
  public measureIntervalHandle: any;
  public componentDidMount() {
    return;
  }

  public componentWillUnmount() {
    return;
  }

  public render() {
    const {
      gridToolbarView,
      gridView,
      gridSetup,
      gridTopology,
      gridCursorView,
      gridInteractionActions,
      gridInteractionSelectors,
      gridOrderingSelectors,
      gridOrderingActions,
      formView,
      formSetup,
      formTopology,
      formActions
    } = this.props.gridPaneBacking;

    const isActiveView =
      gridInteractionSelectors.activeView === GridViewType.Grid;
    return (
      <div
        style={{
          display: !isActiveView ? "none" : "flex",
          flexDirection: "column",
          flex: "1 1 0"
        }}
      >
        <div
          style={{
            display: "flex",
            flexDirection: "column"
          }}
        >
          {/*<ColumnHeaders
            view={gridView}
            columnHeaderRenderer={createColumnHeaderRenderer({
              gridSetup,
              gridOrderingActions,
              gridOrderingSelectors,
              gridTopology
            })}
          />*/}
        </div>
        <Measure client={true}>
          {({ measureRef, contentRect }) => (
            <div
              className="alien"
              ref={measureRef}
              style={{
                flexDirection: "column",
                flex: "1 1 0",
                display:
                  gridInteractionSelectors.activeView === GridViewType.Grid
                    ? "flex"
                    : "none"
              }}
            >
              <Observer>
                {() => (
                  <GridComponent
                    view={gridView}
                    gridSetup={gridSetup}
                    gridTopology={gridTopology}
                    width={contentRect.client!.width || 0}
                    height={contentRect.client!.height || 0}
                    overlayElements={
                      <GridCursorComponent
                        view={gridCursorView}
                        cursorContent={
                          gridInteractionSelectors.activeView ===
                            GridViewType.Grid && (
                            <GridEditorMounter cursorView={gridCursorView}>
                              {gridCursorView.isCellEditing &&
                                React.createElement(
                                  getEditorClass(
                                    gridCursorView.editingRecord,
                                    gridCursorView.editingField
                                  ),
                                  {
                                    editingRecordId: gridCursorView.editingRowId!,
                                    editingFieldId: gridCursorView.editingColumnId!,
                                    editingRecord: gridCursorView.editingRecord,
                                    editingField: gridCursorView.editingField,
                                    value:
                                      gridCursorView.editingOriginalCellValue,
                                    onKeyDown:
                                      gridInteractionActions.handleDumbEditorKeyDown,
                                    onDataCommit:
                                      gridCursorView.handleDataCommit,
                                      cursorPosition: gridCursorView.cursorPosition
                                  }
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
            </div>
          )}
        </Measure>
      </div>
    );
  }
}
