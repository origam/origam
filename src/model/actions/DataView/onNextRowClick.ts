import { selectNextRow } from "./selectNextRow";

export function onNextRowClick(ctx: any) {
  return function onNextRowClick(event: any) {
    selectNextRow(ctx)();
  };
}
