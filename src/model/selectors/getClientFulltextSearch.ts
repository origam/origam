import {getWorkbench} from "./getWorkbench";

export function getClientFullTextSearch(ctx: any) {
  return getWorkbench(ctx).clientFullTextSearch;
}