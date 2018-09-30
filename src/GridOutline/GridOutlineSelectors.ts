import { computed } from "mobx";
import { IGridOutlineState, IGridOutlineSelectors } from "./types";

export class GridOutlineSelectors implements IGridOutlineSelectors {
  constructor(public state: IGridOutlineState) {}

  @computed
  get outlineItems() {
    return this.state.outlineItems;
  }

  @computed
  get lastSelectedItem() {
    return this.state.lastSelectedItem;
  }
}
