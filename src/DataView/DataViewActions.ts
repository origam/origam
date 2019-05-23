import { action, payload, props } from "ts-action";
import { IViewType } from "./types/IViewType";

export const NS = "DataView";



export const selectNextRow = action(`[${NS}] SelectNextRow`);
export const selectPrevRow = action(`[${NS}] SelectPrevRow`);
export const selectNextColumn = action(`[${NS}] SelectNextColumn`);
export const selectPrevColumn = action(`[${NS}] SelectPrevColumn`);

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
);

export const finishEditing = action(`[${NS}] FinishEditing`);
export const cancelEditing = action(`[${NS}] CancelEditing`);
export const startEditing = action(`[${NS}] StartEditing`);
export const dataTableLoaded = action(`[${NS}] DataTableLoaded`);

export const requestSaveData = action(`[${NS}] RequestSaveData`);
export const requestCreateRow = action(`[${NS}] RequestCreateRow`);

export const deleteSelectedRow = action(`[${NS}] DeleteSelectedRow`);
export const createRow = action(`[${NS}] CreateRow`);

export const ACTIVATE_VIEW = `${NS}/ACTIVATE_VIEW`;
export const DEACTIVATE_VIEW = `${NS}/DEACTIVATE_VIEW`;
export const START_DATA_VIEWS = `${NS}/START_DATA_VIEWS`;
export const STOP_DATA_VIEWS = `${NS}/STOP_DATA_VIEWS`;
export const ACTIVATE_INITIAL_VIEW_TYPES = `${NS}/ ACTIVATE_INITIAL_VIEW_TYPES`;

export interface IActivateViewArg {
  viewType: IViewType;
}
export interface IActivateView extends IActivateViewArg {
  type: typeof ACTIVATE_VIEW;
}
export const activateView = (args: IActivateViewArg): IActivateView => ({
  type: ACTIVATE_VIEW,
  ...args
});

export const deactivateView = () => ({ type: DEACTIVATE_VIEW });

export const startDataViews = () => ({ type: START_DATA_VIEWS });

export const stopDataViews = () => ({ type: STOP_DATA_VIEWS });

export const activateInitialViewTypes = () => ({
  type: ACTIVATE_INITIAL_VIEW_TYPES
});
