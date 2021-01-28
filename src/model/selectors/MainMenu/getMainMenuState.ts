import { getMainMenuEnvelope } from "./getMainMenuEnvelope";


export function getMainMenuState(ctx: any) {
  return getMainMenuEnvelope(ctx).mainMenuState;
}
