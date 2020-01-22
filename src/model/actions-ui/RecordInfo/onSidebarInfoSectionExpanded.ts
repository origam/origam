import { flow } from "mobx";
import { getRecordInfo } from "model/selectors/RecordInfo/getRecordInfo";

export function onSidebarInfoSectionExpanded(ctx: any) {
  return flow(function* onSidebarInfoSectionExpanded() {
    yield* getRecordInfo(ctx).onSidebarInfoSectionExpanded();
  });
}
