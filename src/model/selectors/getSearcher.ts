import {getWorkbench} from "./getWorkbench";

export function getSearcher(ctx: any) {
  return getWorkbench(ctx).searcher;
}