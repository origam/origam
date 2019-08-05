import { getWorkbench } from "./getWorkbench";

export function getClientFulltextSearch(ctx: any) {
  return getWorkbench(ctx).clientFulltextSearch;
}