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

import { selectPrevRow } from "model/actions/DataView/selectPrevRow";
import { selectNextRow } from "model/actions/DataView/selectNextRow";
import { selectPrevColumn } from "model/actions/DataView/TableView/selectPrevColumn";
import { selectNextColumn } from "model/actions/DataView/TableView/selectNextColumn";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { getDataView } from "model/selectors/DataView/getDataView";
import { shouldProceedToChangeRow } from "./shouldProceedToChangeRow";
import uiActions from "../../../actions-ui-tree";
import { getDataStructureEntityId } from "model/selectors/DataView/getDataStructureEntityId";
import { getRecordInfo } from "model/selectors/RecordInfo/getRecordInfo";
import { getMenuItemId } from "model/selectors/getMenuItemId";
import { getSessionId } from "model/selectors/getSessionId";

export function onTableKeyDown(ctx: any) {
  return flow(function*onTableKeyDown(event: any) {
    try {
      const dataView = getDataView(ctx);
      switch (event.key) {
        case "ArrowUp":
          event.preventDefault();
          if (!(yield shouldProceedToChangeRow(dataView))) {
            break;
          }
          yield*selectPrevRow(ctx)();

          yield*getRecordInfo(dataView).onSelectedRowMaybeChanged(
            getMenuItemId(dataView),
            getDataStructureEntityId(dataView),
            dataView.selectedRowId,
            getSessionId(dataView)
          );

          getTablePanelView(ctx).scrollToCurrentCell();
          break;
        case "ArrowDown":
          event.preventDefault();
          if (!(yield shouldProceedToChangeRow(dataView))) {
            break;
          }
          yield*selectNextRow(ctx)();

          yield*getRecordInfo(dataView).onSelectedRowMaybeChanged(
            getMenuItemId(dataView),
            getDataStructureEntityId(dataView),
            dataView.selectedRowId,
            getSessionId(dataView)
          );

          getTablePanelView(ctx).scrollToCurrentCell();
          break;
        case "F2":
          getTablePanelView(ctx).setEditing(true);
          event.preventDefault();
          getTablePanelView(ctx).scrollToCurrentCell();
          break;
        case "Tab":
          if (event.shiftKey) {
            yield*selectPrevColumn(ctx)(true);
          } else {
            yield*selectNextColumn(ctx)(true);
          }
          event.preventDefault();
          getTablePanelView(ctx).scrollToCurrentCell();
          break;
        case "Enter":
          if (dataView.firstEnabledDefaultAction) {
            uiActions.actions.onActionClick(dataView.firstEnabledDefaultAction)(
              event,
              dataView.firstEnabledDefaultAction
            );
          }
          break;
        case "Escape": {
          getTablePanelView(ctx).setEditing(false);
          getTablePanelView(ctx).clearCurrentCellEditData();
          getTablePanelView(ctx).triggerOnFocusTable();
          break;
        }
      }
    } catch (e) {
      yield*handleError(ctx)(e);
      throw e;
    }
  });
}
