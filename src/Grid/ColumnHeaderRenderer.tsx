import * as React from "react";
import { Observer } from "mobx-react";
import { IGridSetup, IGridTopology } from "./types";
import {
  IGridOrderingSelectors,
  IGridOrderingActions
} from "src/GridOrdering/types";
import { GridOrderingSelectors } from "src/GridOrdering/GridOrderingSelectors";
import { IFieldId } from "src/DataTable/types";

export function createColumnHeaderRenderer({
  gridSetup,
  gridOrderingActions,
  gridOrderingSelectors,
  gridTopology
}: {
  gridSetup: IGridSetup;
  gridTopology: IGridTopology;
  gridOrderingSelectors: IGridOrderingSelectors;
  gridOrderingActions: IGridOrderingActions;
}): (({ columnIndex }: { columnIndex: number }) => React.ReactNode) {
  function handleHeaderClick(event: any, columnId: IFieldId | undefined): void {
    if (columnId) {
      if (event.ctrlKey) {
        gridOrderingActions.cycleOrderByPreserving(columnId);
      } else {
        gridOrderingActions.cycleOrderByExclusive(columnId);
      }
    }
  }
  return ({ columnIndex }: { columnIndex: number }) => {
    const columnId = gridTopology.getColumnIdByIndex(columnIndex);
    const ordering = columnId
      ? gridOrderingSelectors.getColumnOrdering(columnId)
      : { direction: undefined, order: undefined };
    return (
      <Observer>
        {() => (
          <React.Fragment>
            <div
              style={{}}
              className="table-column-header has-marker"
              onClick={event => 0}
            >
              {gridSetup.getColumnLabel(columnIndex)}{" "}
              <button
                className="ordering-marker"
                onClick={(event: any) => handleHeaderClick(event, columnId)}
              >
                {gridOrderingSelectors.ordering.length > 1 &&
                ordering.order !== undefined
                  ? ordering.order + 1
                  : ""}
                {ordering.direction === "desc" && (
                  <i className={"fa fa-sort-down"} />
                )}
                {ordering.direction === "asc" && (
                  <i className={"fa fa-sort-up"} />
                )}
                {!ordering.direction && <i className={"fa fa-sort"} />}
              </button>
            </div>
            {columnIndex === 4 && (
              <div
                style={{}}
                className="table-column-header"
                onClick={event => 0}
              >
                {gridSetup.getColumnLabel(columnIndex)}
              </div>
            )}
          </React.Fragment>
        )}
      </Observer>
    );
  };
}
