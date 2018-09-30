import { action, observable } from "mobx";
import { IGridOrderingState } from "./types";

export class GridOrderingState implements IGridOrderingState {
  @observable public ordering: Array<[string, string]> = [];

  @action.bound
  public setOrdering(columnId: string, ordering: string) {
    const existing = this.ordering.find(o => o[0] === columnId);
    if(existing) {
      existing[1] = ordering;
    } else {
      this.ordering.push([columnId, ordering]);
    }
  }

  @action.bound public clearOrdering() {
    this.ordering = [];
  }
}