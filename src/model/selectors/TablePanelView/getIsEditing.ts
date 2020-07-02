import {getTablePanelView} from './getTablePanelView';

export function getIsEditing(ctx: any) {
  return getTablePanelView(ctx).isEditing;
}