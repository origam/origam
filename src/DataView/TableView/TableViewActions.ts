export const NS = "TableView";
// TODO: New mediator for table view or change this to DataView NS ???

export const ON_NO_CELL_CLICK = `${NS}/ON_NO_CELL_CLICK`;
export const ON_OUTSIDE_TABLE_CLICK = `${NS}/ON_OUTSIDE_TABLE_CLICK`;
export const ON_CELL_CLICK = `${NS}/ON_CELL_CLICK`;
export const ON_PREV_ROW_CLICK = `${NS}/ON_PREV_ROW_CLICK`;
export const ON_NEXT_ROW_CLICK = `${NS}/ON_NEXT_ROW_CLICK`;

export const SELECT_CELL_BY_ID = `${NS}/SELECT_CELL_BY_ID`;
export const SELECT_CELL_BY_IDX = `${NS}/SELECT_CELL_BY_IDX`;
export const SELECT_FIRST_CELL = `${NS}/SELECT_FIRST_CELL`;
export const SELECT_NEXT_ROW = `${NS}/SELECT_NEXT_ROW`;
export const SELECT_PREV_ROW = `${NS}/SELECT_PREV_ROW`;
export const SELECT_NEXT_COLUMN = `${NS}/SELECT_NEXT_COLUMN`;
export const SELECT_PREV_COLUMN = `${NS}/SELECT_PREV_COLUMN`;
export const MAKE_CELL_VISIBLE_BY_IDX = `${NS}/MAKE_CELL_VISIBLE_BY_IDX`;
export const MAKE_CELL_VISIBLE_BY_ID = `${NS}/MAKE_CELL_VISIBLE_BY_ID`;
export const MAKE_SELECTED_CELL_VISIBLE = `${NS}/MAKE_SELECTED_CELL_VISIBLE`;

export const onPrevRowClick = () => ({ NS, type: ON_PREV_ROW_CLICK });
export const onNextRowClick = () => ({ NS, type: ON_NEXT_ROW_CLICK });

export interface ISelectCellByIdArg {
  rowId: string | undefined;
  columnId: string | undefined;
}
export interface ISelectCellById {
  NS: typeof NS;
  type: typeof SELECT_CELL_BY_ID;
}
export const selectCellById = (args: ISelectCellByIdArg): ISelectCellById => ({
  NS,
  type: SELECT_CELL_BY_ID,
  ...args
});


export interface ISelectCellByIdxArg {
  rowIdx: number | undefined;
  columnIdx: number | undefined;
}
export interface ISelectCellByIdx {
  NS: typeof NS;
  type: typeof SELECT_CELL_BY_ID;
}
export const selectCellByIdx = (args: ISelectCellByIdxArg): ISelectCellByIdx => ({
  NS,
  type: SELECT_CELL_BY_IDX,
  ...args
});

export const selectFirstCell = () => ({ NS, type: SELECT_FIRST_CELL });
export const selectNextRow = () => ({ NS, type: SELECT_NEXT_ROW });
export const selectPrevRow = () => ({ NS, type: SELECT_PREV_ROW });
export const selectNextColumn = () => ({ NS, type: SELECT_NEXT_COLUMN });
export const selectPrevColumn = () => ({ NS, type: SELECT_PREV_COLUMN });

export interface IMakeCellVisibleByIdxArg {
  rowIdx: number;
  columnIdx: number;
}
export interface IMakeCellVisibleByIdx extends IMakeCellVisibleByIdxArg {
  NS: typeof NS;
  type: typeof MAKE_CELL_VISIBLE_BY_IDX;
}
export const makeCellVisibleByIdx = (
  args: IMakeCellVisibleByIdxArg
): IMakeCellVisibleByIdx => ({
  NS,
  type: MAKE_CELL_VISIBLE_BY_IDX,
  ...args
});

export interface IMakeCellVisibleByIdArg {
  rowId: string;
  columnId: string;
}
export interface IMakeCellVisibleById extends IMakeCellVisibleByIdArg {
  NS: typeof NS;
  type: typeof MAKE_CELL_VISIBLE_BY_ID;
}
export const makeCellVisibleById = (
  args: IMakeCellVisibleByIdArg
): IMakeCellVisibleById => ({
  NS,
  type: MAKE_CELL_VISIBLE_BY_ID,
  ...args
});

export const makeSelectedCellVisible = () => ({
  NS,
  type: MAKE_SELECTED_CELL_VISIBLE
});

export const onNoCellClick = () => ({ NS, type: ON_NO_CELL_CLICK });

export const onOutsideTableClick = () => ({ NS, type: ON_OUTSIDE_TABLE_CLICK });

export interface IOnCellClickArg {
  rowIdx: number;
  columnIdx: number;
}
export interface IOnCellClick extends IOnCellClickArg {
  type: typeof ON_CELL_CLICK;
  NS: typeof NS;
}
export const onCellClick = (args: IOnCellClickArg): IOnCellClick => ({
  NS,
  type: ON_CELL_CLICK,
  ...args
});
