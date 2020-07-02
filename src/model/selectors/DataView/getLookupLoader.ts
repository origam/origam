import {getDataView} from "./getDataView";

export function getLookupLoader(ctx: any) {
  return getDataView(ctx).lookupLoader;
}