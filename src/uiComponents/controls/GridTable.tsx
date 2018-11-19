import { observer, Observer, inject } from "mobx-react";
import * as React from "react";
import { IGridPanelBacking } from "src/GridPanel/types";
import { ColumnHeaders, GridComponent } from "src/Grid/GridComponent";
import { createColumnHeaderRenderer } from "src/Grid/ColumnHeaderRenderer";
import { GridViewType } from "src/Grid/types";
import { AutoSizer } from "react-virtualized";
import { GridCursorComponent } from "src/Grid/GridCursorComponent";
import { GridEditorMounter } from "src/cells/GridEditorMounter";
import { StringGridEditor } from "src/cells/string/GridEditor";
import { createGridCellRenderer } from "src/Grid/GridCellRenderer";

import Measure from "react-measure";
import { action, observable } from "mobx";
import * as _ from "lodash";

@inject("gridPaneBacking")
@observer
export class GridTable extends React.Component<any> {
  @observable public width = 0;
  @observable public height = 0;
  @observable public isGridHidden = false;

  public elmMeasure: any;

  @action.bound
  public handleResizeImm(contentRect: any) {
    this.width = (contentRect.client && contentRect.client.width) || 0;
    this.height = (contentRect.client && contentRect.client.height) || 0;
    console.log(this.width, this.height);
  }

  public handleResizeDel = _.throttle(this.handleResizeImm, 100);

  @action.bound
  public showGridImm() {
    this.isGridHidden = false;
  }

  public showGridDel = _.debounce(this.showGridImm, 200, { leading: false });

  @action.bound
  public handleResize(contentRect: any) {
    console.log(contentRect.client);
    this.handleResizeImm(contentRect);
    return;
    this.isGridHidden = true;
    this.showGridDel();
  }

  @action.bound
  public refMeasure(element: any) {
    this.elmMeasure = element;
  }

  public measureIntervalHandle: any;
  public componentDidMount() {
    this.elmMeasure.measure();
    this.measureIntervalHandle = setInterval(() => {
      return;
    }, 1000);
  }

  public componentWillUnmount() {
    clearInterval(this.measureIntervalHandle);
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
          <ColumnHeaders
            view={gridView}
            columnHeaderRenderer={createColumnHeaderRenderer({
              gridSetup,
              gridOrderingActions,
              gridOrderingSelectors,
              gridTopology
            })}
          />
        </div>
        <Measure
          ref={this.refMeasure}
          client={true}
          onResize={this.handleResize}
        >
          {measureStruct => {
            const { width, height } = this;
            return (
              <div
                ref={measureStruct.measureRef}
                className="alien"
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
                      width={width}
                      height={height}
                      overlayElements={
                        <GridCursorComponent
                          view={gridCursorView}
                          cursorContent={
                            gridInteractionSelectors.activeView ===
                              GridViewType.Grid && (
                              <GridEditorMounter cursorView={gridCursorView}>
                                {gridCursorView.isCellEditing && (
                                  <StringGridEditor
                                    editingRecordId={
                                      gridCursorView.editingRowId!
                                    }
                                    editingFieldId={
                                      gridCursorView.editingColumnId!
                                    }
                                    value={
                                      gridCursorView.editingOriginalCellValue
                                    }
                                    onKeyDown={
                                      gridInteractionActions.handleDumbEditorKeyDown
                                    }
                                    onDataCommit={
                                      gridCursorView.handleDataCommit
                                    }
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
                      onNoCellClick={
                        gridInteractionActions.handleGridNoCellClick
                      }
                    />
                  )}
                </Observer>
              </div>
            );
          }}
        </Measure>
      </div>
    );
  }
}
