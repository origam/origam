import React from "react";
import { action, computed } from "mobx";

import { getTablePanelView } from "../../selectors/TablePanelView/getTablePanelView";
import { getDialogStack } from "../../selectors/DialogStack/getDialogStack";
import { IColumnConfigurationDialog } from "./types/IColumnConfigurationDialog";
import {
  ColumnsDialog,
} from "gui/Components/Dialogs/ColumnsDialog";
import { onColumnConfigurationSubmit } from "model/actions-ui/ColumnConfigurationDialog/onColumnConfigurationSubmit";
import { getGroupingConfiguration } from "model/selectors/TablePanelView/getGroupingConfiguration";
import {isLazyLoading} from "model/selectors/isLazyLoading";
import {getFormScreenLifecycle} from "model/selectors/FormScreen/getFormScreenLifecycle";
import { TableConfiguration, TableColumnConfiguration } from "xmlInterpreters/screenXml";
import { ITableColumnsConf } from "./types/IConfigurationManager";


export class ColumnConfigurationDialog implements IColumnConfigurationDialog {
  @computed get columnsConfiguration() {

    const columnConfigurations = []
    const groupingConf = getGroupingConfiguration(this);
    const groupingOnClient = !isLazyLoading(this);
    for (let prop of this.tablePanelView.allTableProperties) {

      const columnConfiguration = new TableColumnConfiguration(prop.id);
      columnConfiguration.name =  prop.name;
      columnConfiguration.isVisible = !this.tablePanelView.hiddenPropertyIds.get(prop.id);
      columnConfiguration.groupingIndex = groupingConf.groupingSettings.get(prop.id)?.groupIndex || 0;
      columnConfiguration.aggregationType = this.tablePanelView.aggregations.getType(prop.id)!;
      columnConfiguration.entity = prop.entity;
      columnConfiguration.canGroup = groupingOnClient ||
        (!prop.isAggregatedColumn && !prop.isLookupColumn && prop.column !== "TagInput");
      columnConfiguration.canAggregate = groupingOnClient ||
        (!prop.isAggregatedColumn && !prop.isLookupColumn && prop.column !== "TagInput");
      columnConfiguration.timeGroupingUnit = groupingConf.groupingSettings.get(prop.id)?.groupingUnit;
      columnConfiguration. width = 0;
      columnConfigurations.push(columnConfiguration);
    }

    return new TableConfiguration({
      name: undefined,
      fixedColumnCount: this.tablePanelView.fixedColumnCount,
      columnConf: columnConfigurations,
      tablePropertyIds: this.tablePanelView.tablePropertyIds
    });
  }

  dialogKey = "";
  dialogId = 0;

  @action.bound
  onColumnConfClick(event: any): void {
    this.dialogKey = `ColumnConfigurationDialog@${this.dialogId++}`;
    getDialogStack(this).pushDialog(
      this.dialogKey,
      <ColumnsDialog
        configuration={this.columnsConfiguration}
        onCancelClick={this.onColumnConfCancel}
        onSaveAsClick={this.onSaveAsClick}
        onCloseClick={this.onColumnConfCancel}
        onOkClick={onColumnConfigurationSubmit(this.tablePanelView)}
      />
    );
  }

  @action.bound onColumnConfCancel(event: any): void {
    getDialogStack(this).closeDialog(this.dialogKey);
  }

  @action.bound onSaveAsClick(event: any, configuration: ITableColumnsConf): void {
    this.ApplyConfiguration(configuration);

    getDialogStack(this).closeDialog(this.dialogKey);
  }

  @action.bound onColumnConfSubmit(event: any, configuration: ITableColumnsConf): void {
    this.ApplyConfiguration(configuration);
    getDialogStack(this).closeDialog(this.dialogKey);
  }

  private ApplyConfiguration(configuration: ITableColumnsConf) {
    this.tablePanelView.fixedColumnCount = configuration.fixedColumnCount;
    this.tablePanelView.hiddenPropertyIds.clear();
    const groupingConf = getGroupingConfiguration(this);
    groupingConf.clearGrouping();
    for (let column of configuration.columnConf) {
      this.tablePanelView.hiddenPropertyIds.set(column.id, !column.isVisible);
      if (column.groupingIndex) {
        groupingConf.setGrouping(column.id, column.timeGroupingUnit, column.groupingIndex);
      }
      this.tablePanelView.aggregations.setType(column.id, column.aggregationType);
    }
    getFormScreenLifecycle(this).loadInitialData();
  }

  @computed get tablePanelView() {
    return getTablePanelView(this);
  }

  parent?: any;
}
