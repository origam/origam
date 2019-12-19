import { getDataView } from "./getDataView";

export function getSelectionMember(ctx: any) {
  return getDataView(ctx).selectionMember;
}
