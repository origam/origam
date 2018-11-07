import { computed } from "mobx";
import { IGridOrderingState, IGridOrderingSelectors } from "./types";

export class GridOrderingSelectors implements IGridOrderingSelectors {
  constructor(public state: IGridOrderingState) {}

  @computed
  public get ordering() {
    return this.state.ordering.filter(o => o[1] !== undefined);
  }

  @computed
  public get orderingsByColumnId() {
    return new Map(
      (this.state.ordering as any).map((o: [string, string], i: number) => [
        o[0],
        { order: i, direction: o[1] }
      ])
    ) as Map<string, { order: number; direction: string }>;
  }

  public getColumnOrdering(columnId: string) {
    return (this.orderingsByColumnId.get(columnId) || {order: undefined, direction: undefined});
  }

  public cycleDirection(direction: string | undefined) {
    switch (direction) {
      case "asc":
        return "desc";
      case "desc":
        return undefined;
      case undefined:
      default:
        return "asc";
    }
  }
}
