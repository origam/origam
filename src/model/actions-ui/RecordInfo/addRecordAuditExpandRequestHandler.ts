import { getRecordInfo } from "model/selectors/RecordInfo/getRecordInfo";

export function addRecordAuditExpandRequestHandler(ctx: any) {
  return function addRecordAuditExpandRequestHandler(handler: () => void) {
    getRecordInfo(ctx).addAuditSectionExpandHandler(handler);
  }
}