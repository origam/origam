import { selectNextColumn } from "../../../actions/DataView/TableView/selectNextColumn";
import { selectPrevColumn } from "../../../actions/DataView/TableView/selectPrevColumn";
import { selectPrevRow } from "../../../actions/DataView/selectPrevRow";
import { selectNextRow } from "../../../actions/DataView/selectNextRow";
import { flushCurrentRowData } from "../../../actions/DataView/TableView/flushCurrentRowData";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { getDataView } from "../../../selectors/DataView/getDataView";
import { shouldProceedToChangeRow } from "model/actions-ui/DataView/TableView/shouldProceedToChangeRow";


export function onFieldKeyDown(ctx: any) {

  function isGoingToChangeRow(tabEvent: any){
    return  (getTablePanelView(ctx).isFirstColumnSelected() && tabEvent.shiftKey) ||
            (getTablePanelView(ctx).isLastColumnSelected() && !tabEvent.shiftKey)
  }
  
  return flow(function* onFieldKeyDown(event: any) {
    try {
      const dataView = getDataView(ctx);

      switch (event.key) {
        case "Tab": {
          if (isGoingToChangeRow(event)){
            getTablePanelView(ctx).setEditing(false);
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
            getTablePanelView(ctx).scrollToCurrentCell();
            getTablePanelView(ctx).setEditing(true);
          }
          else
          {
            if (event.shiftKey) {
              selectPrevColumn(ctx)(true);
            } else {
              selectNextColumn(ctx)(true);
            }
            event.preventDefault();

            getTablePanelView(ctx).scrollToCurrentCell();
            yield* flushCurrentRowData(ctx)();
          }
          break;
        }
        case "Enter": {
          getTablePanelView(ctx).setEditing(false);
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

          getTablePanelView(ctx).setEditing(true);
          getTablePanelView(ctx).triggerOnFocusTable();
          getTablePanelView(ctx).scrollToCurrentCell();
          break;
        }
        case "F2":
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
