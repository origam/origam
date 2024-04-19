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

import { selectNextColumn } from "model/actions/DataView/TableView/selectNextColumn";
import { selectPrevColumn } from "model/actions/DataView/TableView/selectPrevColumn";
import { selectPrevRow } from "model/actions/DataView/selectPrevRow";
import { selectNextRow } from "model/actions/DataView/selectNextRow";
import { flushCurrentRowData } from "model/actions/DataView/TableView/flushCurrentRowData";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { getDataView } from "model/selectors/DataView/getDataView";
import { shouldProceedToChangeRow } from "model/actions-ui/DataView/TableView/shouldProceedToChangeRow";
import { getGridFocusManager } from "model/entities/GridFocusManager";
import {
  isSaveShortcut,
  isAddRecordShortcut,
  isDeleteRecordShortcut,
  isDuplicateRecordShortcut,
  isFilterRecordShortcut, isRefreshShortcut
} from "utils/keyShortcuts";
import { onDeleteRowClick } from "model/actions-ui/DataView/onDeleteRowClick";
import { onCreateRowClick } from "model/actions-ui/DataView/onCreateRowClick";
import { onCopyRowClick } from "model/actions-ui/DataView/onCopyRowClick";
import { onFilterButtonClick } from "model/actions-ui/DataView/onFilterButtonClick";
import { onEscapePressed } from "model/actions-ui/DataView/onEscapePressed";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { getScreenActionButtonsState } from "model/actions-ui/ScreenToolbar/saveButtonVisible";
import { getIsAddButtonVisible } from "model/selectors/DataView/getIsAddButtonVisible";
import { getIsDelButtonVisible } from "model/selectors/DataView/getIsDelButtonVisible";
import { getIsCopyButtonVisible } from "model/selectors/DataView/getIsCopyButtonVisible";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";

export function onFieldKeyDown(ctx: any) {

  function isGoingToChangeRow(tabEvent: any) {
    return (getTablePanelView(ctx).isFirstColumnSelected() && tabEvent.shiftKey) ||
      (getTablePanelView(ctx).isLastColumnSelected() && !tabEvent.shiftKey)
  }

  return flow(function*onFieldKeyDown(event: any) {
    try {
      const dataView = getDataView(ctx);
      const tablePanelView = getTablePanelView(ctx);
      const gridFocusManager = getGridFocusManager(tablePanelView);
      tablePanelView.handleEditorKeyDown(event);
      switch (event.key) {
        case "Tab": {
          if (isGoingToChangeRow(event)) {
            tablePanelView.setEditing(false);
            yield*flushCurrentRowData(ctx)();

            if (!(yield shouldProceedToChangeRow(dataView))) {
              return;
            }

            if (event.shiftKey) {
              selectPrevColumn(ctx)(true);
            } else {
              yield*selectNextColumn(ctx)(true);
            }
            yield*dataView.lifecycle.runRecordChangedReaction();

            event.preventDefault();
            tablePanelView.dontHandleNextScroll();
            tablePanelView.scrollToCurrentCell();
            tablePanelView.setEditing(true);
            tablePanelView.triggerOnFocusTable();
          } else {
            if (event.shiftKey) {
              selectPrevColumn(ctx)(true);
            } else {
              yield*selectNextColumn(ctx)(true);
            }
            event.preventDefault();

            tablePanelView.dontHandleNextScroll();
            tablePanelView.scrollToCurrentCell();
            yield*flushCurrentRowData(ctx)();
            gridFocusManager.focusEditor();
          }
          break;
        }
        case "Enter": {
          let isLastRow = getDataTable(ctx).getLastRow() === getSelectedRow(ctx);
          event.persist?.();
          event.preventDefault();

          yield*flushCurrentRowData(ctx)();

          if (!(yield shouldProceedToChangeRow(dataView))) {
            return;
          }

          if (event.shiftKey) {
            yield*selectPrevRow(ctx)();
            isLastRow = false;
          } else {
            yield*selectNextRow(ctx)();
          }
          yield*dataView.lifecycle.runRecordChangedReaction();

          tablePanelView.scrollToCurrentCell();
          tablePanelView.setEditing(!isLastRow);
          break;
        }
        case "F2": {
          tablePanelView.setEditing(false);
          gridFocusManager.activeEditor = undefined;
          tablePanelView.triggerOnFocusTable();
          break;
        }
        case "Escape": {
          if(!event.closedADropdown){
            yield onEscapePressed(dataView, event);
            tablePanelView.setEditing(false);
            gridFocusManager.activeEditor = undefined;
            tablePanelView.clearCurrentCellEditData();
            tablePanelView.triggerOnFocusTable();
          }
          break;
        }
        default: {
          if (isSaveShortcut(event)) {
            yield*flushCurrentRowData(ctx)();
            if (getScreenActionButtonsState(ctx)?.isSaveButtonVisible) {
              const formScreenLifecycle = getFormScreenLifecycle(ctx);
              yield*formScreenLifecycle.onSaveSession();
            }
          }
          else if (isRefreshShortcut(event) && getScreenActionButtonsState(ctx)?.isRefreshButtonVisible) {
            tablePanelView.setEditing(false);
            yield*flushCurrentRowData(ctx)();
            const formScreenLifecycle = getFormScreenLifecycle(ctx);
            yield*formScreenLifecycle.onRequestScreenReload();
          }
          else if (isAddRecordShortcut(event) && getIsAddButtonVisible(dataView)) {
            tablePanelView.setEditing(false);
            yield*flushCurrentRowData(ctx)();
            yield onCreateRowClick(dataView)(event);
          }
          else if (isDeleteRecordShortcut(event) && getIsDelButtonVisible(dataView)) {
            yield onDeleteRowClick(dataView)(event);
          }
          else if (isDuplicateRecordShortcut(event) && getIsCopyButtonVisible(dataView)) {
            tablePanelView.setEditing(false);
            yield*flushCurrentRowData(ctx)();
            yield onCopyRowClick(dataView)(event);
          }
          else if (isFilterRecordShortcut(event)) {
            yield onFilterButtonClick(dataView)(event);
          }
        }
      }
    } catch (e) {
      yield*handleError(ctx)(e);
      throw e;
    }
  });
}
