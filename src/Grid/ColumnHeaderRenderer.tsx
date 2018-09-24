import * as React from "react";
import { Observer } from "mobx-react";

export function columnHeaderRenderer({
  columnIndex
}: {
  columnIndex: number;
}): React.ReactNode {
  return (
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
            {columnIndex}
          </div>
        </React.Fragment>
      )}
    </Observer>
  );
}
