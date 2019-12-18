import { flow } from "mobx";
import { getDataViewPropertyById } from "model/selectors/DataView/getDataViewPropertyById";
import { saveColumnConfigurations } from "model/actions/DataView/TableView/saveColumnConfigurations";

export function onColumnWidthChangeFinished(ctx: any) {
  return flow(function* onColumnWidthChangeFinished(id: string, width: number) {
    const prop = getDataViewPropertyById(ctx, id);
    if(prop) {
      yield* saveColumnConfigurations(ctx)();
    }
  });
}