import { selectNextColumn } from "./selectNextColumn";
import { selectPrevColumn } from "./selectPrevColumn";
import { selectPrevRow } from "../selectPrevRow";
import { selectNextRow } from "../selectNextRow";
import { flushCurrentRowData } from "./flushCurrentRowData";

export function onFieldKeyDown(ctx: any) {
  return function onFieldKeyDown(event: any) {
    switch (event.key) {
      case "Tab": {
        if (event.shiftKey) {
          selectPrevColumn(ctx)();
          event.preventDefault();
        } else {
          selectNextColumn(ctx)();
          event.preventDefault();
        }
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
        flushCurrentRowData(ctx)();
        break;
      }
    }
  };
}
