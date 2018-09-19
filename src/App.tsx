import * as React from "react";
import { GridComponent } from "./Grid/GridComponent";
import { GridView } from "./Grid/GridView";
import { GridState } from "./Grid/GridState";
import { GridSelectors } from "./Grid/GridSelectors";
import { GridActions } from "./Grid/GridActions";
import { GridSetup } from "./adapters/GridSetup";
import { GridTopology } from "./adapters/GridTopology";
import { T$4, T$2, ICellRect, ICellInfo } from "./Grid/types";
import { trcpr } from "./utils/canvas";

const gridSetup = new GridSetup();
const gridTopology = new GridTopology();
const gridState = new GridState();
const gridSelectors = new GridSelectors(gridState, gridSetup, gridTopology);
const gridActions = new GridActions(gridState, gridSelectors, gridSetup);

const gridView = new GridView(gridSelectors, gridActions);

class App extends React.Component {
  public render() {
    return (
      <div>
        <GridComponent
          view={gridView}
          width={800}
          height={500}
          overlayElements={null}
          cellRenderer={({
            ctx,
            columnIndex,
            rowIndex,
            cellDimensions,
            events
          }) => {
            ctx.fillStyle = rowIndex % 2 === 0 ? "#ffffff" : "#efefef";
            ctx.fillRect(
              ...(trcpr(
                0,
                0,
                cellDimensions.width,
                cellDimensions.height
              ) as T$4)
            );
            ctx.fillStyle = "black";
            let text;
            /*
            if (columnIndex === 0) {
              text = dataTable.records[rowIndex].name;
            } else if (columnIndex === 1) {
              text = moment(dataTable.records[rowIndex].birth_date).format(
                "DD.MM.YYYY"
              );
            } else if (columnIndex === 2) {
              text = dataTable.records[rowIndex].favorite_color;
            } else if (columnIndex === 3) {
              text = dataTable.records[rowIndex].id;
            } else {
              text = `Cell ${columnIndex};${rowIndex}`;
            }*/
            text = `Cell ${columnIndex};${rowIndex}`;
            ctx.fillText(text, ...(trcpr(15, 15) as T$2));

            events.onClick(
              (event: any, cellRect: ICellRect, cellInfo: ICellInfo) => {
                console.log(cellInfo.rowIndex, cellInfo.columnIndex);
              }
            );
          }}
        />
      </div>
    );
  }
}

export default App;
