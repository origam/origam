import { flow } from "mobx";
import { ITableColumnsConf } from "gui/Components/Dialogs/ColumnsDialog";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { getColumnConfigurationDialog } from "model/selectors/getColumnConfigurationDialog";
import { saveColumnConfigurations } from "model/actions/DataView/TableView/saveColumnConfigurations";

export function onColumnConfigurationSubmit(ctx: any) {
  return flow(function* onColumnConfigurationSubmit(
    event: any,
    configuration: ITableColumnsConf
  ) {
    const columnConfigurationDialog = getColumnConfigurationDialog(ctx);
    columnConfigurationDialog.onColumnConfSubmit(event, configuration);
    yield* saveColumnConfigurations(ctx)();

    // TODO: Error handling
  });
}
