import * as React from "react";
import { AutoSizer } from "react-virtualized";
import { GridTopology } from "./adapters/GridTopology";
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
  new DataTableField("name", "Name", 0, false),
  new DataTableField("birth_date", "Birth date", 1, false),
  new DataTableField("likes_platypuses", "Likes platypuses?", 2, false),
  new DataTableField("city_id", "Lives in", 3, true),
  new DataTableField("favorite_color", "Favorite color", 4, false)
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
  gridInteractionSelectors
);
const gridCursorView = new GridCursorView(
  gridInteractionSelectors,
  gridSelectors
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

const gridSetup = new GridSetup(gridInteractionSelectors, dataTableSelectors);
const gridTopology = new GridTopology();
onConfigureGridSetup.trigger(gridSetup);
onConfigureGridTopology.trigger(gridTopology);

cellScrolling.start();

dataLoadingStrategyActions.start();
dataLoadingStrategyActions.requestLoadFresh();

class App extends React.Component {
  public render() {
    return (
      <div
        style={{
          display: "flex",
          flexDirection: "column",
          width: 800,
          height: 700,
          overflow: "hidden"
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
                      <GridEditorMounter cursorView={gridCursorView}>
                        <StringGridEditor
                          value="ahoj"
                          onKeyDown={
                            gridInteractionActions.handleDumbEditorKeyDown
                          }
                        />
                      </GridEditorMounter>
                    }
                  />
                }
                cellRenderer={createGridCellRenderer({
                  gridSetup,
                  onClick(event, cellRect, cellInfo) {
                    gridInteractionActions.handleGridCellClick(event, {
                      rowId: gridTopology.getRowIdByIndex(cellInfo.rowIndex),
                      columnId: gridTopology.getColumnIdByIndex(
                        cellInfo.columnIndex
                      )
                    });
                  }
                })}
                onKeyDown={gridInteractionActions.handleGridKeyDown}
                onOutsideClick={gridInteractionActions.handleGridOutsideClick}
                onNoCellClick={gridInteractionActions.handleGridNoCellClick}
              />
            )}
          </AutoSizer>
        </div>
      </div>
    );
  }
}

export default App;
