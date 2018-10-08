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
import { IFieldType, IDataTableFieldStruct } from "./DataTable/types";
import { Splitter } from "./uiParts/Splitter/SplitterComponent";
import { IGridTopology, IGridSetup } from "./Grid/types";
import { action } from "mobx";
import { IGridPanelBacking } from "./GridPanel/types";

const personFields = [
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

const cityFields = [
  new DataTableField({
    id: "name",
    label: "Name",
    type: IFieldType.string,
    dataIndex: 0,
    isLookedUp: false
  }),
  new DataTableField({
    id: "inhabitants",
    label: "Inhabitants",
    type: IFieldType.integer,
    dataIndex: 1,
    isLookedUp: false
  }),
]

class GridConfiguration {
  public gridSetup: IGridSetup;
  public gridTopology: IGridTopology;

  @action.bound
  public set(gridSetup: IGridSetup, gridTopology: IGridTopology) {
    this.gridSetup = gridSetup;
    this.gridTopology = gridTopology;
  }
}

function createGridPaneBacking(
  dataTableName: string,
  dataTableFields: IDataTableFieldStruct[]
) {
  const configuration = new GridConfiguration();

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

  const dataLoader = new DataLoader(dataTableName);

  const dataTableState = new DataTableState();

  dataTableState.fields = dataTableFields;

  const dataTableSelectors = new DataTableSelectors(
    dataTableState,
    lookupResolverProvider,
    dataTableName
  );
  const dataTableActions = new DataTableActions(
    dataTableState,
    dataTableSelectors
  );

  const onStartGrid = EventObserver();
  const onStopGrid = EventObserver();

  const gridState = new GridState();
  const gridSelectors = new GridSelectors(
    gridState,
    configuration,
    configuration
  );
  const gridActions = new GridActions(gridState, gridSelectors, configuration);

  const gridView = new GridView(gridSelectors, gridActions);

  const gridInteractionState = new GridInteractionState();
  const gridInteractionSelectors = new GridInteractionSelectors(
    gridInteractionState,
    configuration
  );
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
    dataTableActions,
    configuration,
    configuration
  );

  const cellScrolling = new CellScrolling(
    gridSelectors,
    gridActions,
    gridInteractionSelectors,
    configuration,
    configuration
  );
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
    gridInteractionActions,
    configuration
  );

  const gridSetup = new GridSetup(gridInteractionSelectors, dataTableSelectors);
  const gridTopology = new GridTopology(dataTableSelectors);

  configuration.set(gridSetup, gridTopology);
  /*
  onStartGrid.trigger();

  // gridOrderingActions.setOrdering('name', 'asc');

  dataLoadingStrategyActions.requestLoadFresh();*/
  return {
    gridToolbarView,
    gridView,
    gridSetup,
    gridTopology,
    gridCursorView,
    gridInteractionActions,
    onStartGrid,
    onStopGrid,
    dataLoadingStrategyActions
  };
}

@observer
class GridPane extends React.Component<{
  initialDataTableName: string;
  initialFields: IDataTableFieldStruct[];
}> {
  constructor(props: any) {
    super(props);
    this.gridPanelBacking = createGridPaneBacking(
      this.props.initialDataTableName,
      this.props.initialFields
    );
  }

  private gridPanelBacking: IGridPanelBacking;

  public componentDidMount() {
    this.gridPanelBacking.onStartGrid.trigger();
    this.gridPanelBacking.dataLoadingStrategyActions.requestLoadFresh();
  }

  public render() {
    const {
      gridToolbarView,
      gridView,
      gridSetup,
      gridTopology,
      gridCursorView,
      gridInteractionActions
    } = this.gridPanelBacking;
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
                <GridPane
                  initialDataTableName="city"
                  initialFields={cityFields}
                />
                <GridPane
                  initialDataTableName="person"
                  initialFields={personFields}
                />
              </Splitter>
            )}
          </Observer>
        )}
      </AutoSizer>
    );
  }
}

export default App;
/*
class Splitter extends React.Component<{ width: number; height: number; vertical: boolean; }> {
  public render() {
    return (
      <div
        style={{
          display: "flex",
          width: this.props.width,
          height: this.props.height,
          flexDirection: "row"
        }}
      >
        {React.Children.map(this.props.children, (child, index) => (
          <div key={index} style={{flexGrow: 1}}>{child}</div>
        ))}
      </div>
    );
  }
}

*/
