import { flow } from "mobx";
import { handleError } from "../handleError";
import { openScreenByReferenceAndLookup } from "./openScreenByReferenceAndLookup";



export function onSearchResultClick(ctx: any) {
  return async function onSearchResultClick(dataSourceLookupId: string, referenceId: string) {
    flow(function* () {
      try {
        yield* openScreenByReferenceAndLookup(ctx)(
          dataSourceLookupId,
          referenceId
        );
      } catch (e) {
        yield* handleError(ctx)(e);
        throw e;
      }
    })();
  }
}
