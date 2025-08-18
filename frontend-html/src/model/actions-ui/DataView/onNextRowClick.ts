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

import { selectNextRow } from "model/actions/DataView/selectNextRow";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { getDataView } from "model/selectors/DataView/getDataView";
import { shouldProceedToChangeRow } from "./TableView/shouldProceedToChangeRow";
import { getGridFocusManager } from "model/entities/GridFocusManager";
import { getFocusManager } from "model/selectors/getFocusManager";
import { getDataStructureEntityId } from "model/selectors/DataView/getDataStructureEntityId";
import { getRecordInfo } from "model/selectors/RecordInfo/getRecordInfo";
import { getMenuItemId } from "model/selectors/getMenuItemId";
import { getSessionId } from "model/selectors/getSessionId";

export function onNextRowClick(ctx: any) {
  return flow(function*onNextRowClick(event: any) {
    try {
      const focusManager = getFocusManager(ctx);
      yield focusManager.activeEditorCloses();
      const dataView = getDataView(ctx);
      if (!(yield shouldProceedToChangeRow(dataView))) {
        return;
      }
      yield*selectNextRow(ctx)();
      yield*getRecordInfo(dataView).onSelectedRowMaybeChanged(
        getMenuItemId(dataView),
        getDataStructureEntityId(dataView),
        dataView.selectedRowId,
        getSessionId(dataView)
      );

    } catch (e) {
      yield*handleError(ctx)(e);
      throw e;
    }
  });
}
