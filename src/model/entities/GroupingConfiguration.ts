import { observable, computed, action, flow } from "mobx";
import { IGroupingConfiguration } from "./types/IGroupingConfiguration";

export class GroupingConfiguration implements IGroupingConfiguration {
  @observable groupingIndices: Map<string, number> = new Map();

  @computed get isGrouping() {
    return this.groupingIndices.size > 0;
  }

  @computed get groupingColumnCount() {
    return this.groupingIndices.size;
  }

  @computed get orderedGroupingColumnIds() {
    const entries = Array.from(this.groupingIndices.entries());
    entries.sort((a, b) => a[1] - b[1]);
    return entries.map((item) => item[0]);
  }


  @action.bound
  setGrouping(columnId: string, groupingIndex: number): void {
    this.groupingIndices.set(columnId, groupingIndex);
  }

  @action.bound
  clearGrouping(): void {
    this.groupingIndices.clear();
  }

  @action.bound
  applyGrouping(): void {

  }

  parent?: any;
}
