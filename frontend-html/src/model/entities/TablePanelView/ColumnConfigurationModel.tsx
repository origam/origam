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
import { IColumnConfigurationModel } from "model/entities/TablePanelView/types/IColumnConfigurationModel";
import { ColumnsDialog, } from "gui/Components/Dialogs/ColumnsDialog";
import { isLazyLoading } from "model/selectors/isLazyLoading";
import { ITableConfiguration } from "./types/IConfigurationManager";
import { runGeneratorInFlowWithHandler, runInFlowWithHandler } from "utils/runInFlowWithHandler";
import { getConfigurationManager } from "model/selectors/TablePanelView/getConfigurationManager";
import { NewConfigurationDialog } from "gui/Components/Dialogs/NewConfigurationDialog";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { saveColumnConfigurations } from "model/actions/DataView/TableView/saveColumnConfigurations";
import { compareStrings } from "utils/string";
import { GroupingUnit } from "model/entities/types/GroupingUnit";
import { tryParseAggregationType } from "model/entities/types/AggregationType";

export interface IColumnOptions {
  canGroup: boolean;
  canAggregate: boolean;
  entity: string;
  name: string;
  modelInstanceId: string;
}

export class ColumnConfigurationModel implements IColumnConfigurationModel {

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
          name: property.name,
          modelInstanceId: property.modelInstanceId
        })
    }
    return optionsMap;
  }

  @computed get columnsConfiguration() {
    this.tableConfigBeforeChanges = this.configManager.activeTableConfiguration.deepClone();
    return this.configManager.activeTableConfiguration;
  }

  dialogKey = "";
  dialogId = 0;

  @action.bound
  onColumnConfClick(event: any): void {
    this.dialogKey = `ColumnConfigurationDialog@${this.dialogId++}`;
    getDialogStack(this).pushDialog(
      this.dialogKey,
      <ColumnsDialog
        model={this}
      />
    );
  }

  get sortedColumnConfigs(){
    return [...this.columnsConfiguration.columnConfigurations].sort(
      (a, b) => {
        const optionA = this.columnOptions.get(a.propertyId)!;
        const optionB = this.columnOptions.get(b.propertyId)!;
        return compareStrings(optionA.name, optionB.name)
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
        self.tableConfigBeforeChanges = undefined;
        yield*saveColumnConfigurations(self)();
      }()
    })
  }

  @action.bound onColumnConfCancel(event: any): void {
    this.revertChanges();
    getDialogStack(this).closeDialog(this.dialogKey);
  }

  private revertChanges() {
    if (!this.tableConfigBeforeChanges) {
      throw new Error("TableConfiguration was not backed up")
    }
    this.configManager.activeTableConfiguration = this.tableConfigBeforeChanges;
  }

  @action.bound onSaveAsClick(event: any, configuration: ITableConfiguration): void {
    const closeDialog = getDialogStack(this).pushDialog(
      "",
      <NewConfigurationDialog
        onOkClick={(name) => {
          runInFlowWithHandler({
            ctx: this,
            action: () => {
              this.revertChanges();
              this.configManager.cloneAndActivate(configuration, name);
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

  @action.bound onColumnConfSubmit(configuration: ITableConfiguration): void {
    const groupingWasOnBefore = this.tablePanelView?.groupingConfiguration.isGrouping;
    configuration.apply(this.tablePanelView);
    const groupingIsOffNow = !this.tablePanelView?.groupingConfiguration.isGrouping;

    if (groupingWasOnBefore && groupingIsOffNow) {
      getFormScreenLifecycle(this).loadInitialData();
    }
    getDialogStack(this).closeDialog(this.dialogKey);
  }

  @action.bound setVisible(rowIndex: number, state: boolean) {
    this.sortedColumnConfigs[rowIndex].isVisible = state;
  }

  @action.bound setGrouping(rowIndex: number, state: boolean, entity: string) {
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

  @action.bound setTimeGroupingUnit(rowIndex: number, groupingUnit: GroupingUnit | undefined) {
    this.sortedColumnConfigs[rowIndex].timeGroupingUnit = groupingUnit;
  }

  @action.bound setAggregation(rowIndex: number, selectedAggregation: any) {
    this.sortedColumnConfigs[rowIndex].aggregationType = tryParseAggregationType(
      selectedAggregation
    );
  }

  @action.bound handleFixedColumnsCountChange(event: any) {
    this.columnsConfiguration.fixedColumnCount = parseInt(event.target.value, 10);
  }

  @computed get tablePanelView() {
    return getTablePanelView(this);
  }

  get configManager() {
    return getConfigurationManager(this);
  }

  parent?: any;
}
