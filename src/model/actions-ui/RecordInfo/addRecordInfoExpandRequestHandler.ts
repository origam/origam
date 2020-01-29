import { getRecordInfo } from "model/selectors/RecordInfo/getRecordInfo"

export function addRecordInfoExpandRequestHandler(ctx: any) {
  return function addRecordInfoExpandRequestHandler(handler: () => void) {
    return getRecordInfo(ctx).addInfoSectionExpandHandler(handler);
  }
}