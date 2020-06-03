import { observable, computed, action, flow } from "mobx";
import { IGroupingConfiguration } from "./types/IGroupingConfiguration";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getGrouper } from "model/selectors/DataView/getGrouper";

export class GroupingConfiguration implements IGroupingConfiguration {
  @observable groupingIndices: Map<string, number> = new Map();
  onOffHandlers: (()=>void)[] = [];

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

  @computed get firstGroupingColumn() {
    return this.orderedGroupingColumnIds[0];
  }

  nextColumnToGroupBy(columnId: string){
    const currentIndex = this.groupingIndices.get(columnId);
    if(!currentIndex){
      return undefined
    }
    const nextIndex = currentIndex + 1;
    const nextEntry = Array.from(this.groupingIndices.entries())
      .find(entry => entry[1] === nextIndex);
    return nextEntry ? nextEntry[0] : undefined 
  }  

  @action.bound
  setGrouping(columnId: string, groupingIndex: number): void {
    const wasEmpty = this.groupingIndices.size === 0
    this.groupingIndices.set(columnId, groupingIndex);
    if(wasEmpty){
      this.notifyOnOffHandlers();
    }
  }

  @action.bound
  clearGrouping(): void {
    this.groupingIndices.clear();
    this.notifyOnOffHandlers();
  }

  // @action.bound
  // applyGrouping(): void {
  //   getGrouper(this).apply(this.firstGroupingColumn)
  // }

  parent?: any;

  notifyOnOffHandlers(){
    for (let handler of this.onOffHandlers) {
      handler();
    }
  }

  registerGroupingOnOffHandler(handler: () => void): void {
    this.onOffHandlers.push(handler);
  }
}
