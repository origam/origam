import { getRowStates } from "./getRowStates";

export function getRowStateMayCauseFlicker(ctx: any) {
  return getRowStates(ctx).mayCauseFlicker;
}