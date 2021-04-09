import React from "react";
import {action, computed} from "mobx";
import {getDialogStack} from "../../selectors/DialogStack/getDialogStack";
import {IColumnConfigurationDialog} from "./types/IColumnConfigurationDialog";
import {ColumnsDialog,} from "gui/Components/Dialogs/ColumnsDialog";
import {isLazyLoading} from "model/selectors/isLazyLoading";
import {ITableConfiguration} from "./types/IConfigurationManager";
import {runGeneratorInFlowWithHandler, runInFlowWithHandler} from "utils/runInFlowWithHandler";
import {getConfigurationManager} from "model/selectors/TablePanelView/getConfigurationManager";
import {NewConfigurationDialog} from "gui/Components/Dialogs/NewConfigurationDialog";
import {getFormScreenLifecycle} from "model/selectors/FormScreen/getFormScreenLifecycle";
import {getTablePanelView} from "model/selectors/TablePanelView/getTablePanelView";
import {saveColumnConfigurations} from "model/actions/DataView/TableView/saveColumnConfigurations";

export interface IColumnOptions {
  canGroup: boolean;
  canAggregate: boolean;
  entity: string;
  name: string;
}

export class ColumnConfigurationDialog implements IColumnConfigurationDialog {

  tableConfigBeforeChanges: ITableConfiguration | undefined;

  getColumnOptions(){
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
        columnOptions={this.getColumnOptions()}
        configuration={this.columnsConfiguration}
        onCancelClick={this.onColumnConfCancel}
        onSaveAsClick={this.onSaveAsClick}
        onCloseClick={this.onColumnConfCancel}
        onOkClick={this.onColumnConfigurationSubmit.bind(this)}
      />
    );
  }

  @action.bound
  onColumnConfigurationSubmit(configuration: ITableConfiguration) {
    const self = this;
    runGeneratorInFlowWithHandler({
      ctx: this,
      generator: function* (){
        self.onColumnConfSubmit(configuration);
        self.tableConfigBeforeChanges = undefined;
        yield* saveColumnConfigurations(self)();
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
              this.onColumnConfigurationSubmit(this.configManager.activeTableConfiguration);
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

    if(groupingWasOnBefore && groupingIsOffNow){
      getFormScreenLifecycle(this).loadInitialData();
    }
    getDialogStack(this).closeDialog(this.dialogKey);
  }

  @computed get tablePanelView() {
    return getTablePanelView(this);
  }

  get configManager(){
    return getConfigurationManager(this);
  }

  parent?: any;
}
