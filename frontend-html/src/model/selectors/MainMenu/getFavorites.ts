import { getWorkbench } from "model/selectors/getWorkbench";

export function getFavorites(ctx: any) {
  return getWorkbench(ctx).favorites;
}