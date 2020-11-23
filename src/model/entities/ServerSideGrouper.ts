import { getGroupingConfiguration } from "model/selectors/TablePanelView/getGroupingConfiguration";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getDataView } from "model/selectors/DataView/getDataView";
import { IGrouper } from "./types/IGrouper";
import { IReactionDisposer, observable, reaction, comparer, flow} from "mobx";
import { IGroupTreeNode } from "gui/Components/ScreenElements/Table/TableRendering/types";
import { ServerSideGroupItem } from "gui/Components/ScreenElements/Table/TableRendering/GroupItem";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { getOrderingConfiguration } from "model/selectors/DataView/getOrderingConfiguration";
import { joinWithAND, joinWithOR, toFilterItem } from "./OrigamApiHelpers";
import { parseAggregations } from "./Aggregatioins";
import { getUserFilters } from "model/selectors/DataView/getUserFilters";
import { getFilterConfiguration } from "model/selectors/DataView/getFilterConfiguration";
import { IProperty } from "./types/IProperty";
import { getAllLoadedValuesOfProp, getRowById } from "./GrouperCommon";

export class ServerSideGrouper implements IGrouper {
  @observable.shallow topLevelGroups: IGroupTreeNode[] = [];
  parent?: any = null;
  disposers: IReactionDisposer[] = [];
  groupDisposers: Map<IGroupTreeNode, IReactionDisposer> =  new Map<IGroupTreeNode, IReactionDisposer>()
  @observable refreshTrigger = 0;

  start() {
    this.disposers.push(
      reaction(
        () => [
          Array.from(getGroupingConfiguration(this).groupingIndices.values()),
          Array.from(getGroupingConfiguration(this).groupingIndices.keys()),
          this.refreshTrigger],
        () => {
          const self = this;
          flow(function* () {yield* self.loadGroups()})();
        },
        {fireImmediately: true, equals: comparer.structural,delay: 50})
    );
  }

  get allGroups(){
    return this.topLevelGroups.flatMap(group => [group, ...group.allChildGroups]);
  } 

  private *loadGroups() {
    const firstGroupingColumn = getGroupingConfiguration(this).firstGroupingColumn;
    if (!firstGroupingColumn) {
      this.topLevelGroups.length = 0;
      return;
    }
    const expandedGroupDisplayValues = this.allGroups
      .filter(group => group.isExpanded)
      .map(group => group.columnDisplayValue)
    const dataView = getDataView(this);
    const property = getDataTable(this).getPropertyById(firstGroupingColumn);
    const lookupId = property && property.lookup && property.lookup.lookupId;
    const aggregations = getTablePanelView(this).aggregations.aggregationList;
    yield getFormScreenLifecycle(this)
      .loadGroups(dataView, firstGroupingColumn, lookupId, aggregations)
      .then( groupData =>
            this.topLevelGroups = this.group(groupData, firstGroupingColumn, undefined));
    yield* this.loadAndExpandChildren(this.topLevelGroups, expandedGroupDisplayValues);
  }

  private *loadAndExpandChildren(childGroups: IGroupTreeNode[], expandedGroupDisplayValues: string[]): Generator {
    for (const group of childGroups) {
      if( expandedGroupDisplayValues.includes(group.columnDisplayValue)){
        group.isExpanded = true;
        yield* this.loadChildren(group);
        yield* this.loadAndExpandChildren(group.childGroups, expandedGroupDisplayValues)
      }
    }
  }

  refresh() {
    this.refreshTrigger++;
  }

  getRowById(id: string): any[] | undefined {
    return getRowById(this, id);
  }

  notifyGroupClosed(group: IGroupTreeNode){
    if(this.groupDisposers.has(group)){
      this.groupDisposers.get(group)!();
      this.groupDisposers.delete(group);
    }
  }

  *loadChildren(groupHeader: IGroupTreeNode) {
    if(this.groupDisposers.has(groupHeader)){
      this.groupDisposers.get(groupHeader)!();
    }
    this.groupDisposers.set(
      groupHeader,
      reaction(
        ()=> [
          getGroupingConfiguration(this).nextColumnToGroupBy(groupHeader.columnId),
          this.composeFinalFilter(groupHeader),
          [ ...getFilterConfiguration(groupHeader).activeFilters],
          [ ...getTablePanelView(this).aggregations.aggregationList],
          getOrderingConfiguration(this).groupChildrenOrdering
        ], 
        ()=> this.reload(groupHeader),
        // { fireImmediately: true }
      )
    );
    yield* this.reload(groupHeader);
  }

  private *reload(group: IGroupTreeNode) {
    const groupingConfiguration = getGroupingConfiguration(this);
    const nextColumnName = groupingConfiguration.nextColumnToGroupBy(group.columnId);
    const dataView = getDataView(this);
    const filter = this.composeFinalFilter(group);
    const lifeCycle = getFormScreenLifecycle(this);
    const aggregations = getTablePanelView(this).aggregations.aggregationList;
    const orderingConfiguration = getOrderingConfiguration(this);
    if (nextColumnName) {
      const property = getDataTable(this).getPropertyById(nextColumnName);
      const lookupId = property && property.lookup && property.lookup.lookupId;
      yield lifeCycle
        .loadChildGroups(dataView, filter, nextColumnName, aggregations, lookupId)
        .then(
          (groupData) => (group.childGroups = this.group(groupData, nextColumnName, group))
        );
    } else {
      yield lifeCycle
        .loadChildRows(dataView, filter, orderingConfiguration.groupChildrenOrdering)
        .then((rows) => (group.childRows = rows));
    }
  }


  composeFinalFilter(rowGroup: IGroupTreeNode){
    const groupingFilter = rowGroup.composeGroupingFilter();
    const userFilters = getUserFilters(this);

    return userFilters 
      ? joinWithAND([groupingFilter, userFilters])
      : groupingFilter;
  }


  group(groupData: any[], columnId: string, parent: IGroupTreeNode | undefined): IGroupTreeNode[] {
    const groupingConfiguration = getGroupingConfiguration(this);
    const level = groupingConfiguration.groupingIndices.get(columnId);

    if (!level) {
      throw new Error("Cannot find grouping index for column: " + columnId);
    }

    let dataTable = getDataTable(this);
    const property = dataTable.getPropertyById(columnId);

    return groupData.map((groupDataItem) => {
      return new ServerSideGroupItem({
        childGroups: [] as IGroupTreeNode[],
        childRows: [] as any[][],
        columnId: columnId,
        groupLabel: property!.name,
        rowCount: groupDataItem["groupCount"] as number,
        parent: parent,
        columnValue: groupDataItem[columnId],
        columnDisplayValue: groupDataItem["groupCaption"] || groupDataItem[columnId],
        aggregations: parseAggregations(groupDataItem["aggregations"]),
        grouper: this,
      });
    });
  }

  async getAllValuesOfProp(property: IProperty): Promise<Set<any>>{
    const openGroups = this.allGroups
      .filter(group => group.isExpanded && group.childRows.length );
    const infinitelyScrolledGroups = openGroups.filter(group => group.isInfinitelyScrolled);

    let values = await this.getPropValuesFromInfinitelyScrolledGroups(infinitelyScrolledGroups, property);
    return new Set([...getAllLoadedValuesOfProp(property, this), ...values]);
  }

  private async getPropValuesFromInfinitelyScrolledGroups(groups: IGroupTreeNode[], property: IProperty){
    if(groups.length === 0){
      return [];
    }
    const filter = joinWithOR(groups.map(group => this.composeFinalFilter(group)))

    const dataView = getDataView(this);
    const lifeCycle = getFormScreenLifecycle(this);
    const aggregations = getTablePanelView(this).aggregations.aggregationList;

    const lookupId = property && property.lookup && property.lookup.lookupId;
    const groupList = await lifeCycle.loadChildGroups(dataView, filter, property.id, aggregations, lookupId)
    return groupList.map(group => group[property.id]).filter(group => group);
  }

  dispose() {
    for (let disposer of this.disposers) {
      disposer();
    }
  }
}
