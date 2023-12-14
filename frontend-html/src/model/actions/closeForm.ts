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

import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";
import { getOpenedScreen } from "model/selectors/getOpenedScreen";
import { IRefreshOnReturnType } from "../entities/WorkbenchLifecycle/WorkbenchLifecycle";
import { getApi } from "../selectors/getApi";
import { ICRUDResult, processCRUDResult } from "./DataLoading/processCRUDResult";
import { isLazyLoading } from "model/selectors/isLazyLoading";
import { getDataViewLifecycle } from "model/selectors/DataView/getDataViewLifecycle";
import { getFormScreen } from "model/selectors/FormScreen/getFormScreen";
import { onRefreshSessionClick } from "model/actions-ui/ScreenToolbar/onRefreshSessionClick";
import { getMobileState } from "model/selectors/getMobileState";

export function closeForm(ctx: any) {
  return function*closeForm(): Generator {
    const lifecycle = getWorkbenchLifecycle(ctx);
    const openedScreen = getOpenedScreen(ctx);
    getMobileState(ctx).onFormClose(openedScreen?.content?.formScreen);
    yield*lifecycle.closeForm(openedScreen);
    if (openedScreen.content?.refreshOnReturnType) {
      const refreshOnReturnType = openedScreen.content.refreshOnReturnType;
      const parentScreen = getOpenedScreen(openedScreen.parentContext);
      const parentFormScreen = getFormScreen(openedScreen.parentContext);
      switch (refreshOnReturnType) {
        case IRefreshOnReturnType.ReloadActualRecord:
          if (isLazyLoading(ctx)) {
            break;
          }
          for (let dataView of parentFormScreen.dataViews) {
            const dataViewLifecycle = getDataViewLifecycle(dataView);
            yield*dataViewLifecycle.runRecordChangedReaction();
          }
          break;
        case IRefreshOnReturnType.RefreshCompleteForm:
          yield onRefreshSessionClick(parentFormScreen)();
          break;
        case IRefreshOnReturnType.MergeModalDialogChanges:
          const api = getApi(ctx);
          const parentScreenSessionId = parentScreen!.content!.formScreen!.sessionId;
          const changes = (yield api.pendingChanges({sessionFormIdentifier: parentScreenSessionId})) as ICRUDResult[];
          yield*processCRUDResult(openedScreen.parentContext, changes);
          const dataViews = parentScreen?.content?.formScreen?.dataViews ?? [];
          for (const dataView of dataViews) {
            dataView.dataTable.unlockAddedRowPosition();
            yield dataView.dataTable.updateSortAndFilter();
          }
          break;
      }
    }
  };
}
