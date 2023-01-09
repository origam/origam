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

import { flow } from "mobx";
import { getDataView } from "../../selectors/DataView/getDataView";
import { getGridId } from "../../selectors/DataView/getGridId";
import { getEntity } from "../../selectors/DataView/getEntity";
import { getFormScreenLifecycle } from "../../selectors/FormScreen/getFormScreenLifecycle";
import { handleError } from "../../actions/handleError";
import { shouldProceedToChangeRow } from "./TableView/shouldProceedToChangeRow";
import { getFilterConfiguration } from "model/selectors/DataView/getFilterConfiguration";
import { getFocusManager } from "model/selectors/getFocusManager";

export function onCopyRowClick(ctx: any, switchToFormPerspective?: boolean) {
  return flow(function*onCopyRowClick(event: any) {
    try {
      const focusManager = getFocusManager(ctx);
      yield focusManager.activeEditorCloses();
      const selectedRowId = getDataView(ctx).selectedRowId;
      if (!selectedRowId) {
        return;
      }
      const gridId = getGridId(ctx);
      const entity = getEntity(ctx);
      const formScreenLifecycle = getFormScreenLifecycle(ctx);
      const dataView = getDataView(ctx);
      if (!(yield shouldProceedToChangeRow(dataView))) {
        return;
      }
      if(switchToFormPerspective){
        dataView.activateFormView?.({saveNewState: false});
      }
      yield*formScreenLifecycle.onCopyRow(entity, gridId, selectedRowId);
    } catch (e) {
      yield*handleError(ctx)(e);
      throw e;
    }
  });
}