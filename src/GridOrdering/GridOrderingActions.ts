import { action } from "mobx";
import { IGridOrderingState, IGridOrderingSelectors, IGridOrderingActions } from "./types";

export class GridOrderingActions implements IGridOrderingActions {
  constructor(
    public state: IGridOrderingState,
    public selectors: IGridOrderingSelectors
  ) {}

  @action.bound
  public cycleOrderByExclusive(columnId: string) {
    const oldOrdering = this.selectors.getColumnOrdering(columnId);
    const newOrdering = this.selectors.cycleDirection(oldOrdering.direction);
    this.state.clearOrdering();
    this.state.setOrdering(columnId, newOrdering);
  }

  @action.bound
  public cycleOrderByPreserving(columnId: string) {
    const oldOrdering = this.selectors.getColumnOrdering(columnId);
    const newOrdering = this.selectors.cycleDirection(oldOrdering.direction);
    this.state.setOrdering(columnId, newOrdering);
  }

  @action.bound
  public setOrdering(columnId: string, direction: string | undefined) {
    this.state.clearOrdering();
    this.state.setOrdering(columnId, direction);
  }
}
