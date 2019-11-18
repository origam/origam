import { getColumnConfigurationDialog } from "model/selectors/getColumnConfigurationDialog";

export function onColumnConfigurationClick(ctx: any) {
  return function onColumnConfigurationClick(event: any) {
    getColumnConfigurationDialog(ctx).onColumnConfClick(event);
  };
}
