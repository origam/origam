import React from "react";
import { action, computed } from "mobx";

import { getTablePanelView } from "../../selectors/TablePanelView/getTablePanelView";
import { getDialogStack } from "../../selectors/DialogStack/getDialogStack";
import { IColumnConfigurationDialog } from "./types/IColumnConfigurationDialog";
import {
  ITableColumnsConf,
  ColumnsDialog
} from "gui/Components/Dialogs/ColumnsDialog";
import { onColumnConfigurationSubmit } from "model/actions-ui/ColumnConfigurationDialog.tsx/onColumnConfigurationSubmit";

export class ColumnConfigurationDialog implements IColumnConfigurationDialog {
  @computed get columnsConfiguration() {
    const conf: ITableColumnsConf = {
      fixedColumnCount: this.tablePanelView.fixedColumnCount,
      columnConf: []
    };
    for (let prop of this.tablePanelView.allTableProperties) {
      conf.columnConf.push({
        id: prop.id,
        name: prop.name,
        isVisible: !this.tablePanelView.hiddenPropertyIds.get(prop.id),
        groupingIndex: this.tablePanelView.groupingIndices.get(prop.id) || 0,
        aggregation: ""
      });
    }
    return conf;
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
        onCloseClick={this.onColumnConfCancel}
        onOkClick={onColumnConfigurationSubmit(this.tablePanelView)}
      />
    );
  }

  @action.bound onColumnConfCancel(event: any): void {
    getDialogStack(this).closeDialog(this.dialogKey);
  }

  @action.bound onColumnConfSubmit(
    event: any,
    configuration: ITableColumnsConf
  ): void {
    this.tablePanelView.fixedColumnCount = configuration.fixedColumnCount;
    this.tablePanelView.hiddenPropertyIds.clear();
    for (let column of configuration.columnConf) {
      this.tablePanelView.hiddenPropertyIds.set(column.id, !column.isVisible);
    }
    getDialogStack(this).closeDialog(this.dialogKey);
  }

  @computed get tablePanelView() {
    return getTablePanelView(this);
  }

  parent?: any;
}
