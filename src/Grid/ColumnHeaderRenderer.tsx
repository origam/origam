import * as React from "react";
import { Observer } from "mobx-react";
import { IGridSetup } from "./types";

export function createColumnHeaderRenderer({
  gridSetup
}: {
  gridSetup: IGridSetup;
}): (({ columnIndex }: { columnIndex: number }) => React.ReactNode) {
  return ({ columnIndex }: { columnIndex: number }) => (
    <Observer>
      {() => (
        <React.Fragment>
          <div
            style={{
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              height: 20,
              borderRight: "2px solid #cccccc",
              boxSizing: "border-box"
            }}
            onClick={event => 0}
          >
            {gridSetup.getColumnLabel(columnIndex)}
          </div>
        </React.Fragment>
      )}
    </Observer>
  );
}
