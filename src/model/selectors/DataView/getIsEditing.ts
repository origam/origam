import {getDataView} from "./getDataView";

export function getIsEditing(ctx: any) {
  return getDataView(ctx).isEditing;
}