import {flow} from "mobx";
import {getRecordInfo} from "model/selectors/RecordInfo/getRecordInfo";

export function onSidebarInfoSectionCollapsed(ctx: any) {
  return flow(function* onSidebarInfoSectionCollapsed() {
    yield* getRecordInfo(ctx).onSidebarInfoSectionCollapsed();
  });
}
