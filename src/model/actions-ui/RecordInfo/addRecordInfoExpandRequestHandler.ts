import { getRecordInfo } from "model/selectors/RecordInfo/getRecordInfo"

export function addRecordInfoExpandRequestHandler(ctx: any) {
  return function addRecordInfoExpandRequestHandler(handler: () => void) {
    getRecordInfo(ctx).addInfoSectionExpandHandler(handler);
  }
}