import { action, payload, props } from "ts-action";

export const NS = "TableView";
// TODO: New mediator for table view or change this to DataView NS ???

export const ON_NO_CELL_CLICK = `${NS}/ON_NO_CELL_CLICK`;
export const ON_OUTSIDE_TABLE_CLICK = `${NS}/ON_OUTSIDE_TABLE_CLICK`;
export const ON_CELL_CLICK = `${NS}/ON_CELL_CLICK`;

export const SELECT_FIRST_CELL = `${NS}/SELECT_FIRST_CELL`;

export const selectFirstCell = () => ({ type: SELECT_FIRST_CELL });

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

export const onNoCellClick = () => ({ type: ON_NO_CELL_CLICK });

export const onOutsideTableClick = () => ({ type: ON_OUTSIDE_TABLE_CLICK });

export interface IOnCellClickArg {
  rowIdx: number;
  columnIdx: number;
}
export interface IOnCellClick extends IOnCellClickArg {
  type: typeof ON_CELL_CLICK;
}
export const onCellClick = (args: IOnCellClickArg): IOnCellClick => ({
  type: ON_CELL_CLICK,
  ...args
});
