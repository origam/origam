import { getOrderingConfiguration } from "model/selectors/DataView/getOrderingConfiguration";
import { runInAction } from "mobx";

export function onColumnHeaderClick(ctx: any) {
  return function onColumnHeaderClick(event: any, column: string) {
    runInAction(() => {
      if (event.ctrlKey) {
        getOrderingConfiguration(ctx).addOrdering(column);
      } else {
        getOrderingConfiguration(ctx).setOrdering(column);
      }
    });
  };
}
