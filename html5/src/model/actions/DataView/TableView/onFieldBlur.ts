import { flushCurrentRowData } from "./flushCurrentRowData";

export function onFieldBlur(ctx: any) {
  return function onFieldBlur(event: any) {
    flushCurrentRowData(ctx)();
  };
}
