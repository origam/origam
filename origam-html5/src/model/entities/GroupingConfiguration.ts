/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import {action, computed, observable} from "mobx";
import {IGroupingConfiguration, IGroupingSettings} from "./types/IGroupingConfiguration";
import { GroupingUnit } from "./types/GroupingUnit";

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
