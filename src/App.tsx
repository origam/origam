import * as React from "react";
import { GridComponent } from "./Grid/GridComponent";
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
const cellScrolling = new CellScrolling(gridSelectors, gridActions, gridTopology, gridSetup, gridInteractionSelectors);
cellScrolling.start();

gridInteractionState.setSelected("3", "2");

class App extends React.Component {
  public render() {
    return (
      <div>
        <GridComponent
          view={gridView}
          width={800}
          height={500}
          overlayElements={
            <GridCursorComponent view={gridCursorView} cursorContent={null} />
          }
          cellRenderer={createGridCellRenderer({
            onClick(event, cellRect, cellInfo) {
              gridInteractionActions.handleGridCellClick(event, {
                rowId: gridTopology.getRowIdByIndex(cellInfo.rowIndex),
                columnId: gridTopology.getColumnIdByIndex(cellInfo.columnIndex)
              });
            }
          })}
          onKeyDown={gridInteractionActions.handleGridKeyDown}
        />
      </div>
    );
  }
}

export default App;
