import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";
import { isInfiniteScrollingActive } from "model/selectors/isInfiniteScrollingActive";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { IDataView } from "model/entities/types/IDataView";

export async function shouldProceedToChangeRow(dataView: IDataView) {
  const isDirty = getFormScreen(dataView).isDirty;
  if (isDirty && isInfiniteScrollingActive(dataView)) {
    return await getFormScreenLifecycle(dataView).handleUserInputOnChangingRow(dataView);
  }
  return true;
}
