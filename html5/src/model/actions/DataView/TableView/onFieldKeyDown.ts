import { selectNextColumn } from "./selectNextColumn";
import { selectPrevColumn } from "./selectPrevColumn";
import { selectPrevRow } from "../selectPrevRow";
import { selectNextRow } from "../selectNextRow";
import { flushCurrentRowData } from "./flushCurrentRowData";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { runInAction } from "mobx";

export function onFieldKeyDown(ctx: any) {
  return function onFieldKeyDown(event: any) {
    runInAction(() => {
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
          flushCurrentRowData(ctx)();
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
          flushCurrentRowData(ctx)();
          break;
        }
        case "Escape": {
          getTablePanelView(ctx).setEditing(false);
          getTablePanelView(ctx).triggerOnFocusTable();
          break;
        }
      }
    });
  };
}
