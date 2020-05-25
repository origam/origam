import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { getDataSourceFieldByName } from "model/selectors/DataSources/getDataSourceFieldByName";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getIsSelectionCheckboxesShown } from "model/selectors/DataView/getIsSelectionCheckboxesShown";
import { getSelectionMember } from "model/selectors/DataView/getSelectionMember";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import actions from "model/actions-tree";
import actionsUi from "model/actions-ui-tree";
import { onPossibleSelectedRowChange } from "model/actions-ui/onPossibleSelectedRowChange";
import { getMenuItemId } from "model/selectors/getMenuItemId";
import { getDataStructureEntityId } from "model/selectors/DataView/getDataStructureEntityId";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import _ from "lodash";
import { getActions } from "model/selectors/DataView/getActions";

export function onTableCellClick(ctx: any) {
  return _.debounce(
    flow(function* onTableCellClick(event: any, rowIndex: number, columnIndex: number) {
      try {
        console.log("click", rowIndex, columnIndex, event.isDouble);
        if (event.isDouble) {
          for (let action of getActions(ctx)) {
            console.log(action);
            if (action.isDefault && action.isEnabled) {
              yield actionsUi.actions.onActionClick(ctx)(event, action);
              return;
            }
          }
        }
        if (getIsSelectionCheckboxesShown(ctx) && columnIndex === -1) {
          // TODO: Move to tablepanelview
          const dataTable = getDataTable(ctx);
          const selectionMember = getSelectionMember(ctx);
          const row = dataTable.getRowByExistingIdx(rowIndex);
          if (!!selectionMember) {
            const dsField = getDataSourceFieldByName(ctx, selectionMember);
            if (dsField) {
              const value = dataTable.getCellValueByDataSourceField(row, dsField);
              dataTable.setDirtyValue(row, selectionMember, !value);
              yield* getFormScreenLifecycle(ctx).onFlushData();
            }
          } else {
            yield* actions.selectionCheckboxes.toggleSelectedId(ctx)(dataTable.getRowId(row));
          }
          return;
        } else {
          yield* getTablePanelView(ctx).onCellClick(event, rowIndex, columnIndex);
          onPossibleSelectedRowChange(ctx)(
            getMenuItemId(ctx),
            getDataStructureEntityId(ctx),
            getSelectedRowId(ctx)
          );
          return;
        }
      } catch (e) {
        yield* handleError(ctx)(e);
        throw e;
      }
    }),
    200
  );
}
