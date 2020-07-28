import {getDataView} from '../../selectors/DataView/getDataView';
import {getTablePanelView} from "../../selectors/TablePanelView/getTablePanelView";

export function selectNextRow(ctx: any) {
  return function* selectNextRow() {
    getDataView(ctx).selectNextRow();
    getTablePanelView(ctx).scrollToCurrentRow();
  }
}