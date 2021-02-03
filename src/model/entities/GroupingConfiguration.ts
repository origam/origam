import {action, computed, observable} from "mobx";
import {GroupingUnit, IGroupingConfiguration, IGroupingSettings} from "./types/IGroupingConfiguration";

export class GroupingConfiguration implements IGroupingConfiguration {
  @observable groupingSettings: Map<string, IGroupingSettings> = new Map();
  onOffHandlers: (()=>void)[] = [];

  @computed get isGrouping() {
    return this.groupingSettings.size > 0;
  }

  @computed get groupingColumnCount() {
    return this.groupingSettings.size;
  }

  @computed get orderedGroupingColumnSettings() {
    const entries = Array.from(this.groupingSettings.entries());
    entries.sort((a, b) => a[1].groupIndex - b[1].groupIndex);
    return entries.map((item) => item[1]);
  }

  @computed get firstGroupingColumn() {
    return this.orderedGroupingColumnSettings[0];
  }

  nextColumnToGroupBy(columnId: string){
    const currentIndex = this.groupingSettings.get(columnId)?.groupIndex;
    if(!currentIndex){
      return undefined
    }
    const nextIndex = currentIndex + 1;
    const nextEntry = Array.from(this.groupingSettings.entries())
      .find(entry => entry[1].groupIndex === nextIndex);
    return nextEntry ? nextEntry[1] : undefined 
  }  

  @action.bound
  setGrouping(columnId: string, groupingUnit: GroupingUnit | undefined, groupingIndex: number): void {
    const wasEmpty = this.groupingSettings.size === 0
    this.groupingSettings.set(
      columnId,
      {
        columnId: columnId, 
        groupingUnit: groupingUnit,
        groupIndex: groupingIndex
      }
    );
    if(wasEmpty){
      this.notifyOnOffHandlers();
    }
  }

  @action.bound
  clearGrouping(): void {
    this.groupingSettings.clear();
    this.notifyOnOffHandlers();
  }

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
