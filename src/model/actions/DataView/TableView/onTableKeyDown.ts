import { selectPrevRow } from "../selectPrevRow";
import { selectNextRow } from "../selectNextRow";
import { selectPrevColumn } from "./selectPrevColumn";
import { selectNextColumn } from "./selectNextColumn";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { flow } from "mobx";

export function onTableKeyDown(ctx: any) {
  return flow(function* onTableKeyDown(event: any) {
    switch (event.key) {
      case "ArrowUp":
        yield* selectPrevRow(ctx)();
        event.preventDefault();
        getTablePanelView(ctx).scrollToCurrentCell();
        break;
      case "ArrowDown":
        yield* selectNextRow(ctx)();
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
      case "Tab":
        if (event.shiftKey) {
          selectPrevColumn(ctx)(true);
        } else {
          selectNextColumn(ctx)(true);
        }
        event.preventDefault();
        getTablePanelView(ctx).scrollToCurrentCell();
        break;
      case "Enter":
        if (event.shiftKey) {
          yield* selectPrevRow(ctx)();
        } else {
          yield* selectNextRow(ctx)();
        }
        getTablePanelView(ctx).scrollToCurrentCell();
        break;
    }
  });
}
