import * as React from "react";
import { GridComponent } from "./Grid/GridComponent";
import { GridView } from "./Grid/GridView";
import { GridState } from "./Grid/GridState";
import { GridSelectors } from "./Grid/GridSelectors";
import { GridActions } from "./Grid/GridActions";

const gridState = new GridState();
const gridSelectors = new GridSelectors(gridState);
const gridActions = new GridActions(gridState, gridSelectors);

const gridView = new GridView(gridSelectors, gridActions);

class App extends React.Component {
  public render() {
    return (
      <div>
        <GridComponent view={gridView} width={800} height={500} overlayElements={null}/>
      </div>
    );
  }
}

export default App;
