import { IViewType } from "./types/IViewType";

export const NS = "DataView";

/*const SELECT_ROW
const SELECT_CELL_BY_ID
const SELECT_CELL_BY_IDX
const SELECT_COLUMN*/

export const FINISH_EDITING = `${NS}/FINISH_EDITING`;
export const CANCEL_EDITING = `${NS}/CANCEL_EDITING`;
export const START_EDITING = `${NS}/START_EDITING`;
export const DATA_TABLE_LOADED = `${NS}/DATA_TABLE_LOADED`;
export const REQUEST_SAVE_DATA = `${NS}/REQUEST_SAVE_DATA`;
export const REQUEST_CREATE_ROW = `${NS}/REQUEST_CREATE_ROW`;
export const DELETE_SELECTED_ROW = `${NS}/DELETE_SELECTED_ROW`;
export const CREATE_ROW = `${NS}/CREATE_ROW`;




/*
export const selectCellById = action(
  `[${NS}] SelectCell`,
  (payload: { rowId: string | undefined; columnId: string | undefined }) => ({
    NS,
    payload
  })
);

export const selectCellByIdx = action(
  `[${NS}] SelectCell`,
  (payload: { rowIdx: number | undefined; columnIdx: number | undefined }) => ({
    NS,
    payload
  })
);

export const selectRow = action(
  `[${NS}] SelectRow`,
  (payload: { rowId: string | undefined }) => ({ NS, payload })
);

export const selectColumn = action(
  `[${NS}] SelectColumn`,
  (
    payload: { rowId: string | undefined } | { rowIdx: number | undefined }
  ) => ({ NS, payload })
);*/


export const ACTIVATE_VIEW = `${NS}/ACTIVATE_VIEW`;
export const DEACTIVATE_VIEW = `${NS}/DEACTIVATE_VIEW`;
export const START_DATA_VIEWS = `${NS}/START_DATA_VIEWS`;
export const STOP_DATA_VIEWS = `${NS}/STOP_DATA_VIEWS`;
export const ACTIVATE_INITIAL_VIEW_TYPES = `${NS}/ ACTIVATE_INITIAL_VIEW_TYPES`;


export const finishEditing = () => ({NS, type: FINISH_EDITING});
export const cancelEditing = () => ({NS, type: CANCEL_EDITING});
export const startEditing = () => ({NS, type: START_EDITING});
export const dataTableLoaded = () => ({NS, type: DATA_TABLE_LOADED});
export const requestSaveData = () => ({NS, type: REQUEST_SAVE_DATA});
export const requestCreateRow = () => ({NS, type: REQUEST_CREATE_ROW});
export const deleteSelectedRow = () => ({NS, type: DELETE_SELECTED_ROW});
export const createRow = () => ({NS, type: CREATE_ROW});

export interface IActivateViewArg {
  viewType: IViewType;
}
export interface IActivateView extends IActivateViewArg {
  NS: typeof NS,
  type: typeof ACTIVATE_VIEW;
}
export const activateView = (args: IActivateViewArg): IActivateView => ({
  NS,
  type: ACTIVATE_VIEW,
  ...args
});

export const deactivateView = () => ({ NS, type: DEACTIVATE_VIEW });

export const startDataViews = () => ({ NS, type: START_DATA_VIEWS });

export const stopDataViews = () => ({ NS, type: STOP_DATA_VIEWS });

export const activateInitialViewTypes = () => ({
  NS, type: ACTIVATE_INITIAL_VIEW_TYPES
});
