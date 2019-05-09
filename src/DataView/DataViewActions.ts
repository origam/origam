import { action, payload, props } from "ts-action";
import { IDataView } from "./types/IDataView";

export const NS = "DataView";

export const selectFirstCell = action(`[${NS}] SelectFirstCell`);

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