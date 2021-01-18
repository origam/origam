import {selectPrevRow} from "../../../actions/DataView/selectPrevRow";
import {selectNextRow} from "../../../actions/DataView/selectNextRow";
import {selectPrevColumn} from "../../../actions/DataView/TableView/selectPrevColumn";
import {selectNextColumn} from "../../../actions/DataView/TableView/selectNextColumn";
import {getTablePanelView} from "model/selectors/TablePanelView/getTablePanelView";
import {flow} from "mobx";
import {handleError} from "model/actions/handleError";
import { getDataView } from "model/selectors/DataView/getDataView";
import { shouldProceedToChangeRow } from "./shouldProceedToChangeRow";

export function onTableKeyDown(ctx: any) {
  return flow(function* onTableKeyDown(event: any) {
    try {
      const dataView = getDataView(ctx);
      switch (event.key) {
        case "ArrowUp":
          if (!(yield shouldProceedToChangeRow(dataView))) {
            break;
          }
          yield* selectPrevRow(ctx)();
          event.preventDefault();
          getTablePanelView(ctx).scrollToCurrentCell();
          break;
        case "ArrowDown":
          if (!(yield shouldProceedToChangeRow(dataView))) {
            break;
          }
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
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
