import { flushCurrentRowData } from "./flushCurrentRowData";
import { flow } from "mobx";

export function onFieldBlur(ctx: any) {
  return flow(function* onFieldBlur(event: any) {
    yield* flushCurrentRowData(ctx)();
  });
}
