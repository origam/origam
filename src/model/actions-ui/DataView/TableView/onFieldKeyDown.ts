import { selectNextColumn } from "../../../actions/DataView/TableView/selectNextColumn";
import { selectPrevColumn } from "../../../actions/DataView/TableView/selectPrevColumn";
import { selectPrevRow } from "../../../actions/DataView/selectPrevRow";
import { selectNextRow } from "../../../actions/DataView/selectNextRow";
import { flushCurrentRowData } from "../../../actions/DataView/TableView/flushCurrentRowData";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { getDataView } from "../../../selectors/DataView/getDataView";
import { isInfiniteScrollingActive } from "../../../selectors/isInfiniteScrollingActive";
import { getFormScreenLifecycle } from "../../../selectors/FormScreen/getFormScreenLifecycle";
import { getFormScreen } from "../../../selectors/FormScreen/getFormScreen";

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
          getTablePanelView(ctx).setEditing(false);
          event.persist?.();
          event.preventDefault();

          yield* flushCurrentRowData(ctx)();

          const dataView = getDataView(ctx);
          const isDirty = getFormScreen(dataView).isDirty;
          if (isDirty && isInfiniteScrollingActive(dataView)) {
            const shouldProceedToChangeRow = yield getFormScreenLifecycle(dataView)
              .handleUserInputOnChangingRow(dataView);
            if (!shouldProceedToChangeRow) {
              return;
            }
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
