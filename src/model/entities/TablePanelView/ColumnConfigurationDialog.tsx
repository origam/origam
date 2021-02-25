import React from "react";
import {action, computed, flow} from "mobx";
import {getDialogStack} from "../../selectors/DialogStack/getDialogStack";
import {IColumnConfigurationDialog} from "./types/IColumnConfigurationDialog";
import {ColumnsDialog,} from "gui/Components/Dialogs/ColumnsDialog";
import {getGroupingConfiguration} from "model/selectors/TablePanelView/getGroupingConfiguration";
import {isLazyLoading} from "model/selectors/isLazyLoading";
import {ITableConfiguration} from "./types/IConfigurationManager";
import {runGeneratorInFlowWithHandler, runInFlowWithHandler} from "utils/runInFlowWithHandler";
import {getConfigurationManager} from "model/selectors/TablePanelView/getConfigurationManager";
import {NewConfigurationDialog} from "gui/Components/Dialogs/NewConfigurationDialog";
import {getFormScreenLifecycle} from "model/selectors/FormScreen/getFormScreenLifecycle";
import {getTablePanelView} from "model/selectors/TablePanelView/getTablePanelView";
import {getColumnConfigurationDialog} from "model/selectors/getColumnConfigurationDialog";
import {saveColumnConfigurations} from "model/actions/DataView/TableView/saveColumnConfigurations";

export interface IColumnOptions {
  canGroup: boolean;
  canAggregate: boolean;
  entity: string;
  name: string;
}

export class ColumnConfigurationDialog implements IColumnConfigurationDialog {
  getColumnOptions(){
    const groupingConf = getGroupingConfiguration(this);
    const groupingOnClient = !isLazyLoading(this);
    const activeTableConfiguration = getConfigurationManager(this).activeTableConfiguration;
    const optionsMap = new Map<string, IColumnOptions>()

    for (let columnConfiguration of activeTableConfiguration.columnConfigurations) {
      const property = this.tablePanelView.allTableProperties
        .find(prop => prop.id === columnConfiguration.propertyId)!;
      optionsMap.set(
        property.id,
        {
          canGroup: groupingOnClient ||
            (!property.isAggregatedColumn && !property.isLookupColumn && property.column !== "TagInput"),
          canAggregate: groupingOnClient ||
            (!property.isAggregatedColumn && !property.isLookupColumn && property.column !== "TagInput"),
          entity: property.entity,
          name: property.name,
        })
    }
    return optionsMap;
  }

  @computed get columnsConfiguration() {
    return getConfigurationManager(this).activeTableConfiguration;
  }

  dialogKey = "";
  dialogId = 0;

  @action.bound
  onColumnConfClick(event: any): void {
    this.dialogKey = `ColumnConfigurationDialog@${this.dialogId++}`;
    getDialogStack(this).pushDialog(
      this.dialogKey,
      <ColumnsDialog
        columnOptions={this.getColumnOptions()}
        configuration={this.columnsConfiguration}
        onCancelClick={this.onColumnConfCancel}
        onSaveAsClick={this.onSaveAsClick}
        onCloseClick={this.onColumnConfCancel}
        onOkClick={this.onColumnConfigurationSubmit.bind(this)}
      />
    );
  }

  onColumnConfigurationSubmit(configuration: ITableConfiguration) {
    const self = this;
    runGeneratorInFlowWithHandler({
      ctx: this,
      generator: function* bla(){
        self.onColumnConfSubmit(configuration);
        yield* saveColumnConfigurations(self)();
      }()
    })
  }

  @action.bound onColumnConfCancel(event: any): void {
    getDialogStack(this).closeDialog(this.dialogKey);
  }

  @action.bound onSaveAsClick(event: any, configuration: ITableConfiguration): void {
     const closeDialog = getDialogStack(this).pushDialog(
      "",
      <NewConfigurationDialog
        onOkClick={(name) => {
          runInFlowWithHandler({
            ctx: this,
            action: () => {
              const configurationManager = getConfigurationManager(this);
              configurationManager.cloneAndActivate(configuration, name);
              this.onColumnConfigurationSubmit(configurationManager.activeTableConfiguration);
            }
          });
          closeDialog();
        }}
        onCancelClick={() => closeDialog()}
      />
    );

    getDialogStack(this).closeDialog(this.dialogKey);
  }

  @action.bound onColumnConfSubmit(configuration: ITableConfiguration): void {
    configuration.apply(this.tablePanelView);
    getFormScreenLifecycle(this).loadInitialData();
    getDialogStack(this).closeDialog(this.dialogKey);
  }

  @computed get tablePanelView() {
    return getTablePanelView(this);
  }

  parent?: any;
}
