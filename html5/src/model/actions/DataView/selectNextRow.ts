import { getDataView } from '../../selectors/DataView/getDataView';

export function selectNextRow(ctx: any) {
  return function selectNextRow() {
    getDataView(ctx).selectNextRow();
  }
}