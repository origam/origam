import {IDataView} from "../../entities/types/IDataView";
import {getOrderingConfiguration} from "./getOrderingConfiguration";
import {getDataStructureEntityId} from "./getDataStructureEntityId";
import {getDataView} from "./getDataView";

export function getUserOrdering(ctx: any) {
  const dataView =  getDataView(ctx);
  const orderingConfiguration = getOrderingConfiguration(dataView);
  const defaultOrdering = orderingConfiguration.getDefaultOrdering();
  if(!defaultOrdering){
    const dataStructureEntityId = getDataStructureEntityId(dataView);
    throw new Error(`Cannot infinitely scroll on dataStructureEntity: ${dataStructureEntityId} because it has no default ordering on the displayed form.`)
  }
  return orderingConfiguration.ordering.length === 0
    ? [defaultOrdering]
    : orderingConfiguration.ordering;
}