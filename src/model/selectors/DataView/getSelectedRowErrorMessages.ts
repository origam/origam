import { getTableViewProperties } from "../TablePanelView/getTableViewProperties";
import { getDataView } from "./getDataView";
import { getFieldErrorMessage } from "./getFieldErrorMessage";

export function getSelectedRowErrorMessages(ctx: any) {
  const dataView = getDataView(ctx);
  if (!dataView.selectedRow) {
    return [];
  }
  return getTableViewProperties(ctx)
    .map(property => getFieldErrorMessage(property)(dataView.selectedRow!, property))
    .filter(message => message);
}
