import {getDataView} from "model/selectors/DataView/getDataView";
import {scopeFor} from "dic/Container";
import {IViewConfiguration} from "modules/DataView/ViewConfiguration";

export function getActivePerspective(ctx: any) {
  const dataView = getDataView(ctx);
  const $cont = scopeFor(dataView);
  const viewConfiguration = $cont && $cont.resolve(IViewConfiguration);
  return viewConfiguration?.activePerspective
}