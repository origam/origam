import {getMainMenuEnvelope} from "./getMainMenuEnvelope";

export function getIsMainMenuLoading(ctx: any) {
  return getMainMenuEnvelope(ctx).isLoading;
}