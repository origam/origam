import { selectPrevRow } from "../selectPrevRow";
import { selectNextRow } from "../selectNextRow";
import { selectPrevColumn } from "./selectPrevColumn";
import { selectNextColumn } from './selectNextColumn';

export function onTableKeyDown(ctx: any) {
  return function onTableKeyDown(event: any) {
    switch (event.key) {
      case "ArrowUp":
        selectPrevRow(ctx)();
        event.preventDefault();
        break;
      case "ArrowDown":
        selectNextRow(ctx)();
        event.preventDefault();
        break;
      case "ArrowLeft":
          selectPrevColumn(ctx)();
          event.preventDefault();
        break;
      case "ArrowRight":
          selectNextColumn(ctx)();
          event.preventDefault();
        break;
    }
  };
}
