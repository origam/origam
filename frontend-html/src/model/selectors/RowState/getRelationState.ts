import { getDataView } from "../DataView/getDataView";
import { getRowStateById } from "./getRowStateById";

export function getRelationState(ctx: any) {
    const dataView = getDataView(ctx);
    const bindingParent = dataView.bindingParent;
    if (bindingParent?.selectedRowId) {
        return getRowStateById(bindingParent, bindingParent!.selectedRowId!)
            ?.relations
            .find(relation => relation.name === dataView.entity);
    }
    return undefined;
}
