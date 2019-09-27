import { selectPrevRow } from "../selectPrevRow";
import { selectNextRow } from "../selectNextRow";
import { selectPrevColumn } from "./selectPrevColumn";
import { selectNextColumn } from "./selectNextColumn";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";

export function onTableKeyDown(ctx: any) {
  return function onTableKeyDown(event: any) {
    switch (event.key) {
      case "ArrowUp":
        selectPrevRow(ctx)();
        event.preventDefault();
        getTablePanelView(ctx).scrollToCurrentCell();
        break;
      case "ArrowDown":
        selectNextRow(ctx)();
        event.preventDefault();
        getTablePanelView(ctx).scrollToCurrentCell();
        break;
      case "ArrowLeft":
        selectPrevColumn(ctx)();
        event.preventDefault();
        getTablePanelView(ctx).scrollToCurrentCell();
        break;
      case "ArrowRight":
        selectNextColumn(ctx)();
        event.preventDefault();
        getTablePanelView(ctx).scrollToCurrentCell();
        break;
      case "F2":
        getTablePanelView(ctx).setEditing(true);
        event.preventDefault();
        getTablePanelView(ctx).scrollToCurrentCell();
        break;
    }
  };
}
