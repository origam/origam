import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";
import { isInfiniteScrollingActive } from "model/selectors/isInfiniteScrollingActive";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { IDataView } from "model/entities/types/IDataView";
import { handleUserInputOnChangingRow } from "model/entities/FormScreenLifecycle/questionSaveDataAfterRecordChange";

export async function shouldProceedToChangeRow(dataView: IDataView) {
  const isDirty = getFormScreen(dataView).isDirty;
  if(!isDirty){
    return true
  }
  return await handleUserInputOnChangingRow(dataView);
}
