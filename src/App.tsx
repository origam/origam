import * as React from "react";
import { AutoSizer } from "react-virtualized";
import { GridEditorMounter } from "./cells/GridEditorMounter";
import { StringGridEditor } from "./cells/string/GridEditor";
import { LookupResolverProvider } from "./DataLoadingStrategy/LookupResolverProvider";
import { DataTableSelectors } from "./DataTable/DataTableSelectors";
import { DataTableState, DataTableField } from "./DataTable/DataTableState";
import { CellScrolling } from "./Grid/CellScrolling";
import { columnHeaderRenderer } from "./Grid/ColumnHeaderRenderer";
import { GridActions } from "./Grid/GridActions";
import { createGridCellRenderer } from "./Grid/GridCellRenderer";
import { ColumnHeaders, GridComponent } from "./Grid/GridComponent";
import { GridCursorComponent } from "./Grid/GridCursorComponent";
import { GridCursorView } from "./Grid/GridCursorView";
import { GridInteractionActions } from "./Grid/GridInteractionActions";
import { GridInteractionSelectors } from "./Grid/GridInteractionSelectors";
import { GridInteractionState } from "./Grid/GridInteractionState";
import { GridSelectors } from "./Grid/GridSelectors";
import { GridState } from "./Grid/GridState";
import { GridView } from "./Grid/GridView";
import { GridSetup } from "./GridPanel/adapters/GridSetup";
import { EventObserver } from "./utils/events";
import { DataLoadingStrategyActions } from "./DataLoadingStrategy/DataLoadingStrategyActions";
import { DataLoadingStrategySelectors } from "./DataLoadingStrategy/DataLoadingStrategySelectors";
import { DataLoadingStrategyState } from "./DataLoadingStrategy/DataLoadingStrategyState";
import { DataTableActions } from "./DataTable/DataTableActions";
import { DataLoader } from "./DataLoadingStrategy/DataLoader";
import { GridOrderingSelectors } from "./GridOrdering/GridOrderingSelectors";
import { GridOrderingState } from "./GridOrdering/GridOrderingState";
import { GridOrderingActions } from "./GridOrdering/GridOrderingActions";
import { GridOutlineState } from "./GridOutline/GridOutlineState";
import { GridOutlineSelectors } from "./GridOutline/GridOutlineSelectors";
import { GridOutlineActions } from "./GridOutline/GridOutlineActions";
import { GridTopology } from "./GridPanel/adapters/GridTopology";
import { Observer, observer } from "mobx-react";
import { GridToolbarView } from "./GridPanel/GridToolbarView";
import { IFieldType } from "./DataTable/types";
import { Splitter } from "./uiParts/Spitter/SplitterComponent";

const lookupResolverProvider = new LookupResolverProvider({
  get dataLoader() {
    return dataLoader;
  }
});

const gridOrderingState = new GridOrderingState();
const gridOrderingSelectors = new GridOrderingSelectors(gridOrderingState);
const gridOrderingActions = new GridOrderingActions(
  gridOrderingState,
  gridOrderingSelectors
);

const gridOutlineState = new GridOutlineState();
const gridOutlineSelectors = new GridOutlineSelectors(gridOutlineState);
const gridOutlineActions = new GridOutlineActions(
  gridOutlineState,
  gridOutlineSelectors
);

const dataLoader = new DataLoader("person");

const dataTableState = new DataTableState();

dataTableState.fields = [
  new DataTableField({
    id: "name",
    label: "Name",
    type: IFieldType.string,
    dataIndex: 0,
    isLookedUp: false
  }),
  new DataTableField({
    id: "birth_date",
    label: "Birth date",
    type: IFieldType.date,
    dataIndex: 1,
    isLookedUp: false
  }),
  new DataTableField({
    id: "likes_platypuses",
    label: "Likes platypuses?",
    type: IFieldType.boolean,
    dataIndex: 2,
    isLookedUp: false
  }),
  new DataTableField({
    id: "city_id",
    label: "Lives in",
    type: IFieldType.string,
    dataIndex: 3,
    isLookedUp: true,
    lookupResultFieldId: "name",
    lookupResultTableId: "city"
  }),
  new DataTableField({
    id: "favorite_color",
    label: "Favorite color",
    type: IFieldType.color,
    dataIndex: 4,
    isLookedUp: false
  })
];

const dataTableSelectors = new DataTableSelectors(
  dataTableState,
  lookupResolverProvider,
  "person"
);
const dataTableActions = new DataTableActions(
  dataTableState,
  dataTableSelectors
);

const onConfigureGridSetup = EventObserver();
const onConfigureGridTopology = EventObserver();
const onStartGrid = EventObserver();
const onStopGrid = EventObserver();

const gridState = new GridState();
const gridSelectors = new GridSelectors(gridState);
onConfigureGridSetup(gs => (gridSelectors.setup = gs));
onConfigureGridTopology(gt => (gridSelectors.topology = gt));
const gridActions = new GridActions(gridState, gridSelectors);
onConfigureGridSetup(gs => (gridActions.setup = gs));
const gridView = new GridView(gridSelectors, gridActions);

const gridInteractionState = new GridInteractionState();
const gridInteractionSelectors = new GridInteractionSelectors(
  gridInteractionState
);
onConfigureGridTopology(gt => (gridInteractionSelectors.gridTopology = gt));
const gridInteractionActions = new GridInteractionActions(
  gridInteractionState,
  gridInteractionSelectors,
  gridActions
);
onStartGrid(() => gridInteractionActions.start());
onStopGrid(() => gridInteractionActions.stop());
const gridCursorView = new GridCursorView(
  gridInteractionSelectors,
  gridSelectors,
  dataTableSelectors,
  dataTableActions
);
onConfigureGridSetup(gs => (gridCursorView.gridSetup = gs));
onConfigureGridTopology(gt => (gridCursorView.gridTopology = gt));
const cellScrolling = new CellScrolling(
  gridSelectors,
  gridActions,
  gridInteractionSelectors
);
onConfigureGridSetup(gs => (cellScrolling.gridSetup = gs));
onConfigureGridTopology(gt => (cellScrolling.gridTopology = gt));
onStartGrid(() => cellScrolling.start());
onStopGrid(() => cellScrolling.stop());

const dataLoadingStrategyState = new DataLoadingStrategyState();
const dataLoadingStrategySelectors = new DataLoadingStrategySelectors(
  dataLoadingStrategyState,
  gridSelectors,
  dataTableSelectors
);
const dataLoadingStrategyActions = new DataLoadingStrategyActions(
  dataLoadingStrategyState,
  dataLoadingStrategySelectors,
  dataTableActions,
  dataTableSelectors,
  dataLoader,
  gridOrderingSelectors,
  gridOrderingActions,
  gridOutlineSelectors,
  gridInteractionActions,
  gridSelectors,
  gridActions
);
onStartGrid(() => dataLoadingStrategyActions.start());
onStopGrid(() => dataLoadingStrategyActions.stop());

const gridToolbarView = new GridToolbarView(
  gridInteractionSelectors,
  gridSelectors,
  dataTableSelectors,
  dataTableActions,
  gridInteractionActions
);
onConfigureGridTopology(gt => (gridToolbarView.gridTopology = gt));

const gridSetup = new GridSetup(gridInteractionSelectors, dataTableSelectors);
const gridTopology = new GridTopology(dataTableSelectors);
onConfigureGridSetup.trigger(gridSetup);
onConfigureGridTopology.trigger(gridTopology);
onStartGrid.trigger();

// gridOrderingActions.setOrdering('name', 'asc');

dataLoadingStrategyActions.requestLoadFresh();

@observer
class GridPane extends React.Component {
  public render() {
    return (
      <AutoSizer>
        {({ width: paneWidth, height: paneHeight }) => (
          <Observer>
            {() => (
              <div
                style={{
                  display: "flex",
                  flexDirection: "column",
                  width: paneWidth,
                  height: paneHeight,
                  overflow: "hidden"
                }}
              >
                <div
                  style={{
                    display: "flex",
                    flexDirection: "row"
                  }}
                >
                  <button onClick={gridToolbarView.handleAddRecordClick}>
                    Add
                  </button>
                  <button onClick={gridToolbarView.handleRemoveRecordClick}>
                    Remove
                  </button>
                </div>
                <div
                  style={{
                    display: "flex",
                    flexDirection: "column"
                  }}
                >
                  <ColumnHeaders
                    view={gridView}
                    columnHeaderRenderer={columnHeaderRenderer}
                  />
                </div>
                <div
                  style={{
                    display: "flex",
                    flexDirection: "column",
                    height: "100%",
                    flex: "1 1"
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
                                  <GridEditorMounter
                                    cursorView={gridCursorView}
                                  >
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
                                }
                              />
                            }
                            cellRenderer={createGridCellRenderer({
                              gridSetup,
                              onClick(event, cellRect, cellInfo) {
                                gridInteractionActions.handleGridCellClick(
                                  event,
                                  {
                                    rowId: gridTopology.getRowIdByIndex(
                                      cellInfo.rowIndex
                                    )!,
                                    columnId: gridTopology.getColumnIdByIndex(
                                      cellInfo.columnIndex
                                    )!
                                  }
                                );
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
                    )}
                  </AutoSizer>
                </div>
              </div>
            )}
          </Observer>
        )}
      </AutoSizer>
    );
  }
}

@observer
class App extends React.Component {
  public render() {
    return (
      <AutoSizer>
        {({ width, height }) => (
          <Observer>
            {() => (
              <Splitter width={width} height={height} vertical={false}>
                <GridPane />
                <div>Hello</div>
              </Splitter>
            )}
          </Observer>
        )}
      </AutoSizer>
    );
  }
}

export default App;
