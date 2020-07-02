import {getTablePanelView} from './getTablePanelView';

export function getTableViewProperties(ctx: any) {
  return getTablePanelView(ctx).tableProperties;
}