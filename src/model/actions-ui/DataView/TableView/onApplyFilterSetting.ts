import {flow, toJS} from "mobx";
import {getProperty} from "model/selectors/DataView/getProperty";
import {getFilterConfiguration} from "../../../selectors/DataView/getFilterConfiguration";
import {handleError} from "model/actions/handleError";

export function onApplyFilterSetting(ctx: any) {
  const prop = getProperty(ctx);
  return flow(function* onApplyFilterSetting(setting: any) {
    try {
      console.log("apply filter:", prop, toJS(setting));
      getFilterConfiguration(ctx).setFilter({ propertyId: prop.id, setting });
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
