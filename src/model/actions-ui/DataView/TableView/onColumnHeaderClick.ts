import {getOrderingConfiguration} from "model/selectors/DataView/getOrderingConfiguration";
import {flow} from "mobx";
import {handleError} from "model/actions/handleError";
import { getProperties } from "model/selectors/DataView/getProperties";

export function onColumnHeaderClick(ctx: any) {
  return flow(function* onColumnHeaderClick(event: any, column: string) {
    try {
      const property = getProperties(ctx).find(prop => prop.id === column);
      if(property?.column === "Blob" || property?.column === "TagInput"){
        return;
      }
      if (event.ctrlKey) {
        getOrderingConfiguration(ctx).addOrdering(column);
      } else {
        getOrderingConfiguration(ctx).setOrdering(column);
      }
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
