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

import React from "react";
import { action, computed } from "mobx";
import { getDialogStack } from "../../selectors/DialogStack/getDialogStack";
import { isLazyLoading } from "model/selectors/isLazyLoading";
import { ITableConfiguration } from "./types/IConfigurationManager";
import {
  runGeneratorInFlowWithHandler,
  runInFlowWithHandler,
} from "utils/runInFlowWithHandler";
import { getConfigurationManager } from "model/selectors/TablePanelView/getConfigurationManager";
import { NewConfigurationDialog } from "gui/Components/Dialogs/NewConfigurationDialog";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { saveColumnConfigurations } from "model/actions/DataView/TableView/saveColumnConfigurations";
import { compareStrings } from "utils/string";
import {
  GroupingUnit,
  groupingUnitToLabel,
} from "model/entities/types/GroupingUnit";
import {
  AggregationType,
  tryParseAggregationType,
} from "model/entities/types/AggregationType";
import { T } from "utils/translation";
import _ from "lodash";
import { getTableViewProperties } from "model/selectors/TablePanelView/getTableViewProperties";
import { IOption } from "gui/Components/Dialogs/SimpleDropdown";

export interface IColumnOptions {
  canGroup: boolean;
  canAggregate: boolean;
  entity: string;
  gridCaption: string;
  modelInstanceId: string;
}

export const dialogKey = "ColumnConfigurationDialog";

export class ColumnConfigurationModel{

  tableConfigBeforeChanges: ITableConfiguration | undefined;

  @computed
  get columnOptions() {
    const groupingOnClient = !isLazyLoading(this);
    const activeTableConfiguration = this.configManager.activeTableConfiguration;
    const optionsMap = new Map<string, IColumnOptions>()

    for (let columnConfiguration of activeTableConfiguration.columnConfigurations) {
      const property = this.tablePanelView.allTableProperties
        .find(prop => prop.id === columnConfiguration.propertyId)!;
      optionsMap.set(
        property.id,
        {
          canGroup: groupingOnClient ||
            (!property.isAggregatedColumn && property.fieldType !== "DetachedField"),
          canAggregate: groupingOnClient ||
            (!property.isAggregatedColumn && !property.isLookupColumn && property.column !== "TagInput"),
          entity: property.entity,
          gridCaption: property.gridCaption,
          modelInstanceId: property.modelInstanceId
        })
    }
    return optionsMap;
  }

  get columnsConfiguration() {
    return this.configManager.activeTableConfiguration;
  }

  reset() {
    this.tableConfigBeforeChanges = this.configManager.activeTableConfiguration.deepClone();
  }

  get sortedColumnConfigs(){
    return [...this.columnsConfiguration.columnConfigurations].sort(
      (a, b) => {
        const optionA = this.columnOptions.get(a.propertyId)!;
        const optionB = this.columnOptions.get(b.propertyId)!;
        return compareStrings(optionA.gridCaption, optionB.gridCaption)
      }
    );
  }

  @action.bound
  onColumnConfigurationSubmit() {
    const self = this;
    runGeneratorInFlowWithHandler({
      ctx: this,
      generator: function*() {
        self.onColumnConfSubmit(self.columnsConfiguration);
        self.reset();
        yield*saveColumnConfigurations(self)();
      }()
    })
  }

  @action.bound
  onColumnConfCancel(): void {
    this.revertChanges();
    getDialogStack(this).closeDialog(dialogKey);
  }

  private revertChanges() {
    if (!this.tableConfigBeforeChanges) {
      throw new Error("TableConfiguration was not backed up")
    }
    this.configManager.activeTableConfiguration = this.tableConfigBeforeChanges;
  }

  @action.bound
  onSaveAsClick(): void {
    const closeDialog = getDialogStack(this).pushDialog(
      "",
      <NewConfigurationDialog
        onOkClick={(name) => {
          runInFlowWithHandler({
            ctx: this,
            action: () => {
              const newConfiguration = this.columnsConfiguration.deepClone();
              this.revertChanges();
              this.configManager.cloneAndActivate(newConfiguration, name);
              this.onColumnConfigurationSubmit();
            }
          });
          closeDialog();
        }}
        onCancelClick={() => {
          this.revertChanges();
          closeDialog();
        }}
      />
    );
  }

  @action.bound
  onColumnConfSubmit(configuration: ITableConfiguration): void {
    const aggregationsBefore = this.tablePanelView?.aggregations.aggregationList;
    const groupingWasOnBefore = this.tablePanelView?.groupingConfiguration.isGrouping;      
    configuration.apply(this.tablePanelView);
    const groupingIsOnNow = this.tablePanelView?.groupingConfiguration.isGrouping;
    const aggregationsNow = this.tablePanelView?.aggregations.aggregationList;
    if (groupingWasOnBefore && !groupingIsOnNow ||
      groupingIsOnNow && !_.isEqual(aggregationsBefore, aggregationsNow))
    {
      getFormScreenLifecycle(this).loadInitialData();
    }
    getDialogStack(this).closeDialog(dialogKey);
  }

  @action.bound
  setVisible(rowIndex: number, state: boolean) {
    this.sortedColumnConfigs[rowIndex].isVisible = state;
  }

  @action.bound
  setGrouping(rowIndex: number, state: boolean, entity: string) {
    if (entity === "Date") {
      if (state) {
        this.setTimeGroupingUnit(rowIndex, GroupingUnit.Day);
      } else {
        this.setTimeGroupingUnit(rowIndex, undefined);
      }
    }

    const columnConfCopy = [...this.sortedColumnConfigs];
    columnConfCopy.sort((a, b) => b.groupingIndex - a.groupingIndex);
    if (this.sortedColumnConfigs[rowIndex].groupingIndex === 0) {
      this.sortedColumnConfigs[rowIndex].groupingIndex =
        columnConfCopy[0].groupingIndex + 1;
    } else {
      this.sortedColumnConfigs[rowIndex].groupingIndex = 0;
      let groupingIndex = 1;
      columnConfCopy.reverse();
      for (let columnConfItem of columnConfCopy) {
        if (columnConfItem.groupingIndex > 0) {
          columnConfItem.groupingIndex = groupingIndex++;
        }
      }
    }
  }

  @action.bound
  setTimeGroupingUnit(rowIndex: number, groupingUnit: GroupingUnit | undefined) {
    this.sortedColumnConfigs[rowIndex].timeGroupingUnit = groupingUnit;
  }

  @action.bound
  setAggregation(rowIndex: number, selectedAggregation: any) {
    this.sortedColumnConfigs[rowIndex].aggregationType = tryParseAggregationType(
      selectedAggregation
    );
  }

  @action.bound
  setWidth(rowIndex: number, width: number) {
    this.sortedColumnConfigs[rowIndex].width = width;
  }

  @action.bound
  handleFixedColumnsCountChange(event: any) {
    this.columnsConfiguration.fixedColumnCount = parseInt(event.target.value, 10);
  }

  @action.bound
  setOrderIds(ids: any[]) {
    const idsSet = new Set(ids);
    const restOfIds = this.columnsConfiguration.columnConfigurations
      .map(item => item.propertyId)
      .filter(id => !idsSet.has(id))

    this.columnsConfiguration.columnConfigurations = [
      ...ids.map(id => this.columnsConfiguration.columnConfigurations.find(item => item.propertyId === id)!), 
      ...restOfIds.map(id => this.columnsConfiguration.columnConfigurations.find(item => item.propertyId === id)!)
    ]
  }

  @computed get tableViewProperties() {
    return getTableViewProperties(this.tablePanelView);
  }

  @computed get tablePanelView() {
    return getTablePanelView(this);
  }

  get configManager() {
    return getConfigurationManager(this);
  }

  parent?: any;
}

export class AggregationOption implements IOption<AggregationType | undefined>{
  constructor (
    public label: string,
    public value: AggregationType | undefined,
  ){
  }
}

export class TimeUnitOption implements IOption<GroupingUnit>{
  constructor (
    public label: string,
    public value: GroupingUnit,
  ){
  }
} 


export const aggregationOptions =  [
  new AggregationOption("", undefined),
  new AggregationOption(T("SUM", "aggregation_sum"), AggregationType.SUM),
  new AggregationOption(T("AVG", "aggregation_avg"), AggregationType.AVG),
  new AggregationOption(T("MIN", "aggregation_min"), AggregationType.MIN),
  new AggregationOption(T("MAX", "aggregation_max"), AggregationType.MAX),
];

export const timeunitOptions =  [
  new TimeUnitOption(groupingUnitToLabel(GroupingUnit.Year), GroupingUnit.Year),
  new TimeUnitOption(groupingUnitToLabel(GroupingUnit.Month), GroupingUnit.Month),
  new TimeUnitOption(groupingUnitToLabel(GroupingUnit.Day), GroupingUnit.Day),
  new TimeUnitOption(groupingUnitToLabel(GroupingUnit.Hour), GroupingUnit.Hour),
  new TimeUnitOption(groupingUnitToLabel(GroupingUnit.Minute), GroupingUnit.Minute),
];
