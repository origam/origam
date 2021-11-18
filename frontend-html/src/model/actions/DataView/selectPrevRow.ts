import {getDataView} from "model/selectors/DataView/getDataView";
import {getTablePanelView} from "../../selectors/TablePanelView/getTablePanelView";

export function selectPrevRow(ctx: any) {
  return function* selectPrevRow() {
    getDataView(ctx).selectPrevRow();
    getTablePanelView(ctx).scrollToCurrentRow();
  };
}
