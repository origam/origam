import {getRowStates} from "./getRowStates";

export function getRowStateById(ctx: any, id: string) {
    return getRowStates(ctx).getValue(id);
}