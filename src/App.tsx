import * as React from "react";
import { GridComponent, ColumnHeaders } from "./Grid/GridComponent";
import { GridView } from "./Grid/GridView";
import { GridState } from "./Grid/GridState";
import { GridSelectors } from "./Grid/GridSelectors";
import { GridActions } from "./Grid/GridActions";
import { GridSetup } from "./adapters/GridSetup";
import { GridTopology } from "./adapters/GridTopology";
import { createGridCellRenderer } from "./Grid/GridCellRenderer";

import { GridCursorComponent } from "./Grid/GridCursorComponent";
import { GridCursorView } from "./Grid/GridCursorView";
import { GridInteractionSelectors } from "./Grid/GridInteractionSelectors";
import { GridInteractionState } from "./Grid/GridInteractionState";
import { GridInteractionActions } from "./Grid/GridInteractionActions";
import { CellScrolling } from "./Grid/CellScrolling";
import { columnHeaderRenderer } from "./Grid/ColumnHeaderRenderer";
import { AutoSizer } from "react-virtualized";

import { StringGridEditor } from "./cells/string/GridEditor";
import { GridEditorMounter } from "./cells/GridEditorMounter";

const gridSetup = new GridSetup();
const gridTopology = new GridTopology();
const gridState = new GridState();
const gridSelectors = new GridSelectors(gridState, gridSetup, gridTopology);
const gridActions = new GridActions(gridState, gridSelectors, gridSetup);

const gridView = new GridView(gridSelectors, gridActions);

const gridInteractionState = new GridInteractionState();
const gridInteractionSelectors = new GridInteractionSelectors(
  gridInteractionState,
  gridTopology
);
const gridInteractionActions = new GridInteractionActions(
  gridInteractionState,
  gridInteractionSelectors
);
const gridCursorView = new GridCursorView(
  gridTopology,
  gridSetup,
  gridInteractionSelectors,
  gridSelectors
);
const cellScrolling = new CellScrolling(
  gridSelectors,
  gridActions,
  gridTopology,
  gridSetup,
  gridInteractionSelectors
);
cellScrolling.start();

gridInteractionState.setSelected("3", "2");

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
                width={width}
                height={height}
                overlayElements={
                  <GridCursorComponent
                    view={gridCursorView}
                    cursorContent={
                      <GridEditorMounter cursorView={gridCursorView}>
                        <StringGridEditor />
                      </GridEditorMounter>
                    }
                  />
                }
                cellRenderer={createGridCellRenderer({
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
