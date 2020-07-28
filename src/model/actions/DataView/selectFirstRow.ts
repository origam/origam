import {getDataView} from "../../selectors/DataView/getDataView";
import {getTablePanelView} from "../../selectors/TablePanelView/getTablePanelView";

export function selectFirstRow(ctx: any) {
  return function* selectFirstRow() {
      getDataView(ctx).selectFirstRow();
      getTablePanelView(ctx).scrollToCurrentRow();
  };
}