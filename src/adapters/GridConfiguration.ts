import { IGridConfiguration, IGridInteractionSelectors } from "../Grid/types";
import { computed } from "mobx";

export class GridConfiguration implements IGridConfiguration {
  constructor(public gridInteractionSelectors: IGridInteractionSelectors) {}
  
  @computed
  get isScrollingEnabled(): boolean {
    return !this.gridInteractionSelectors.isCellEditing;
  }

}