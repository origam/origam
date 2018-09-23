import * as React from "react";
import { observer } from "mobx-react";
import { IGridCursorView } from "./types";

@observer
export class GridCursorComponent extends React.Component<{
  view: IGridCursorView;
  cursorContent: React.ReactNode | React.ReactNode[] | null;
}> {
  public render() {
    const { view, cursorContent } = this.props;
    return (
      <>
        {view.fixedRowCursorDisplayed && (
          <div
            style={{
              position: "absolute",
              overflow: "hidden",
              backgroundColor: "rgba(0,0,0,0.2)",
              ...view.fixedRowCursorStyle
            }}
          >
            {view.fixedCellCursorDisplayed && (
              <div
                style={{
                  position: "absolute",
                  backgroundColor: "rgba(0,0,0,0.2)",
                  ...view.fixedCellCursorStyle
                }}
              >
                {cursorContent}
              </div>
            )}
          </div>
        )}
        {view.movingRowCursorDisplayed && (
          <div
            style={{
              position: "absolute",
              overflow: "hidden",
              backgroundColor: "rgba(0,0,0,0.2)",
              ...view.movingRowCursorStyle
            }}
          >
            {view.movingCellCursorDisplayed && (
              <div
                style={{
                  position: "absolute",
                  backgroundColor: "rgba(0,0,0,0.2)",
                  ...view.movingCellCursorStyle
                }}
              >
                {cursorContent}
              </div>
            )}
          </div>
        )}
      </>
    );
  }
}
