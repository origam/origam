import {getDataView} from "../../selectors/DataView/getDataView";
import {getTablePanelView} from "../../selectors/TablePanelView/getTablePanelView";

export function selectLastRow(ctx: any) {
  return function* selectLastRow() {
    getDataView(ctx).selectLastRow();
    getTablePanelView(ctx).scrollToCurrentRow();
  };
}