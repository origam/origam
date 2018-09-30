import { action } from "mobx";
import { IGridOutlineState, IGridOutlineSelectors, IGridOutlineActions } from "./types";

export class GridOutlineActions implements IGridOutlineActions {
  constructor(
    public state: IGridOutlineState,
    public selectors: IGridOutlineSelectors
  ) {}

  @action.bound
  public setOutlineItems(items: string[]) {
    this.state.setOutlineItems(items);
  }

  @action.bound
  public setLastSelectedItem(item: string) {
    this.state.setLastSelectedItem(item);
  }
}
