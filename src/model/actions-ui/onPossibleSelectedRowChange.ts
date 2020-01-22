import { flow } from "mobx";
import { getRecordInfo } from "model/selectors/RecordInfo/getRecordInfo";

export function onPossibleSelectedRowChange(ctx: any) {
  return flow(function* onPossibleSelectedRowChange(
    menuId: string,
    dataStructureEntityId: string,
    rowId: string | undefined
  ) {
    yield* getRecordInfo(ctx).onSelectedRowMaybeChanged(
      menuId,
      dataStructureEntityId,
      rowId
    );
  });
}
