import { getOrderingConfiguration } from "model/selectors/DataView/getOrderingConfiguration";

export function onColumnHeaderClick(ctx: any) {
  return function onColumnHeaderClick(event: any, column: string) {
    if (event.ctrlKey) {
      getOrderingConfiguration(ctx).addOrdering(column);
    } else {
      getOrderingConfiguration(ctx).setOrdering(column);
    }
    console.log(
      getOrderingConfiguration(ctx).getOrdering("Timestamp"),
      getOrderingConfiguration(ctx).getOrdering("Object")
    );
  };
}
