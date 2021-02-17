import { selectNextColumn } from "model/actions/DataView/TableView/selectNextColumn";
import { selectPrevColumn } from "model/actions/DataView/TableView/selectPrevColumn";
import { selectPrevRow } from "model/actions/DataView/selectPrevRow";
import { selectNextRow } from "model/actions/DataView/selectNextRow";
import { flushCurrentRowData } from "model/actions/DataView/TableView/flushCurrentRowData";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { getDataView } from "model/selectors/DataView/getDataView";
import { shouldProceedToChangeRow } from "model/actions-ui/DataView/TableView/shouldProceedToChangeRow";


export function onFieldKeyDown(ctx: any) {

  function isGoingToChangeRow(tabEvent: any){
    return  (getTablePanelView(ctx).isFirstColumnSelected() && tabEvent.shiftKey) ||
            (getTablePanelView(ctx).isLastColumnSelected() && !tabEvent.shiftKey)
  }
  
  return flow(function* onFieldKeyDown(event: any) {
    try {
      const dataView = getDataView(ctx);
      const tablePanelView = getTablePanelView(ctx);
      console.log(event.key);
      switch (event.key) {
        case "Tab": {
          if (isGoingToChangeRow(event)){
            tablePanelView.setEditing(false);
            yield* flushCurrentRowData(ctx)();

            if (!(yield shouldProceedToChangeRow(dataView))) {
              return;
            }

            yield dataView.lifecycle.runRecordChangedReaction(function*() {
              if (event.shiftKey) {
                yield selectPrevColumn(ctx)(true);
              } else {
                yield selectNextColumn(ctx)(true);
              }
            });

            event.preventDefault();
            tablePanelView.dontHandleNextScroll();
            tablePanelView.scrollToCurrentCell();
            tablePanelView.setEditing(true);
            tablePanelView.triggerOnFocusTable();
          }
          else
          {
            if (event.shiftKey) {
              selectPrevColumn(ctx)(true);
            } else {
              selectNextColumn(ctx)(true);
            }
            event.preventDefault();

            tablePanelView.dontHandleNextScroll();
            tablePanelView.scrollToCurrentCell();
            yield* flushCurrentRowData(ctx)();
          }
          break;
        }
        case "Enter": {
          tablePanelView.setEditing(false);
          event.persist?.();
          event.preventDefault();

          yield* flushCurrentRowData(ctx)();

          if (!(yield shouldProceedToChangeRow(dataView))) {
            return;
          }

          yield dataView.lifecycle.runRecordChangedReaction(function*() {
            if (event.shiftKey) {
              yield* selectPrevRow(ctx)();
            } else {
              yield* selectNextRow(ctx)();
            }
          });

          tablePanelView.setEditing(true);
          tablePanelView.triggerOnFocusTable();
          tablePanelView.scrollToCurrentCell();
          break;
        }
        case "F2": {
          tablePanelView.setEditing(false);
          tablePanelView.triggerOnFocusTable();
          break;
        }
        case "Escape": {
          tablePanelView.setEditing(false);
          tablePanelView.clearCurrentCellEditData();
          tablePanelView.triggerOnFocusTable();
          break;
        }
      }
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
