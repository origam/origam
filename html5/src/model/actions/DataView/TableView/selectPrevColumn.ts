import { getTablePanelView } from '../../../selectors/TablePanelView/getTablePanelView';

export function selectPrevColumn(ctx: any) {
  return function selectPrevColumn() {
    getTablePanelView(ctx).selectPrevColumn();
  }
}