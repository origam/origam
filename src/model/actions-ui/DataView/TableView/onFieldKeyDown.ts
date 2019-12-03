import { selectNextColumn } from "../../../actions/DataView/TableView/selectNextColumn";
import { selectPrevColumn } from "../../../actions/DataView/TableView/selectPrevColumn";
import { selectPrevRow } from "../../../actions/DataView/selectPrevRow";
import { selectNextRow } from "../../../actions/DataView/selectNextRow";
import { flushCurrentRowData } from "../../../actions/DataView/TableView/flushCurrentRowData";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { runInAction, flow } from "mobx";
import { handleError } from "model/actions/handleError";

export function onFieldKeyDown(ctx: any) {
  return flow(function* onFieldKeyDown(event: any) {
    try {
      switch (event.key) {
        case "Tab": {
          if (event.shiftKey) {
            selectPrevColumn(ctx)(true);
            event.preventDefault();
          } else {
            selectNextColumn(ctx)(true);
            event.preventDefault();
          }
          getTablePanelView(ctx).scrollToCurrentCell();
          yield* flushCurrentRowData(ctx)();
          break;
        }
        case "Enter": {
          if (event.shiftKey) {
            selectPrevRow(ctx)();
            event.preventDefault();
          } else {
            selectNextRow(ctx)();
            event.preventDefault();
          }
          getTablePanelView(ctx).scrollToCurrentCell();
          yield* flushCurrentRowData(ctx)();
          break;
        }
        case "Escape": {
          getTablePanelView(ctx).setEditing(false);
          getTablePanelView(ctx).triggerOnFocusTable();
          break;
        }
      }
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
