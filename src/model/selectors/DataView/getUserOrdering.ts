import {getOrderingConfiguration} from "./getOrderingConfiguration";
import {getDataStructureEntityId} from "./getDataStructureEntityId";
import {getDataView} from "./getDataView";

export function getUserOrdering(ctx: any) {
  const dataView =  getDataView(ctx);
  const orderingConfiguration = getOrderingConfiguration(dataView);
  const defaultOrderings = orderingConfiguration.getDefaultOrderings();
  if(defaultOrderings.length === 0){
    const dataStructureEntityId = getDataStructureEntityId(dataView);
    throw new Error(`Cannot infinitely scroll on dataStructureEntity: ${dataStructureEntityId} because it has no default ordering on the displayed form.`)
  }
  return orderingConfiguration.userOrderings.length === 0
    ? defaultOrderings
    : orderingConfiguration.userOrderings;
}