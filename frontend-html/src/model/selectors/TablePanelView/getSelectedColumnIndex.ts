import {getTablePanelView} from './getTablePanelView';

export function getSelectedColumnIndex(ctx: any) {
  return getTablePanelView(ctx).selectedColumnIndex;
}