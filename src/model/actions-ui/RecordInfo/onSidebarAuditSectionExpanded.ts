import { flow } from "mobx";
import { getRecordInfo } from "model/selectors/RecordInfo/getRecordInfo";

export function onSidebarAuditSectionExpanded(ctx: any) {
  return flow(function* onSidebarAuditSectionExpanded() {
    yield* getRecordInfo(ctx).onSidebarAuditSectionExpanded();
  });
}
