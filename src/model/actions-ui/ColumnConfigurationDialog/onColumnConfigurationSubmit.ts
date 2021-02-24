import {flow} from "mobx";
import {getColumnConfigurationDialog} from "model/selectors/getColumnConfigurationDialog";
import {saveColumnConfigurations} from "model/actions/DataView/TableView/saveColumnConfigurations";
import {getTablePanelView} from "model/selectors/TablePanelView/getTablePanelView";
import {getProperties} from "model/selectors/DataView/getProperties";
import { ITableConfiguration } from "model/entities/TablePanelView/types/IConfigurationManager";

export function onColumnConfigurationSubmit(ctx: any) {
  return flow(function* onColumnConfigurationSubmit(
    event: any,
    configuration: ITableConfiguration
  ) {
    const columnConfigurationDialog = getColumnConfigurationDialog(ctx);
    getTablePanelView(ctx).configurationManager.defaultTableConfiguration = configuration;
    columnConfigurationDialog.onColumnConfSubmit(event, configuration);
    yield* saveColumnConfigurations(ctx)();

    // TODO: Error handling
  });
}
