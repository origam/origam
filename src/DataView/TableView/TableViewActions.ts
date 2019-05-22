import { action, payload, props } from "ts-action";

export const NS = "TableView";
// TODO: New mediator for table view or change this to DataView NS ???

export const makeCellVisibleByIdx = action(
  `[${NS}] makeCellVisibleByIdx`,
  (payload: { rowIdx: number; columnIdx: number }) => ({
    NS,
    payload
  })
);

export const makeCellVisibleById = action(
  `[${NS}] makeCellVisibleById`,
  (payload: { rowId: string; columnId: string }) => ({
    NS,
    payload
  })
);

export const makeSelectedCellVisible = action(
  `[${NS}] makeSelectedCellVisible`
);
