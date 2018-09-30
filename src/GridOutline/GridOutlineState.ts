import { observable, action } from "mobx";
import { IGridOutlineState } from "./types";

export class GridOutlineState implements IGridOutlineState {
  @observable
  public outlineItems: string[] = [];

  @observable
  public lastSelectedItem: string | undefined;

  @action.bound
  public setOutlineItems(items: string[]) {
    this.outlineItems = items;
  }

  @action.bound
  public setLastSelectedItem(item: string | undefined) {
    this.lastSelectedItem = item;
  }
}
