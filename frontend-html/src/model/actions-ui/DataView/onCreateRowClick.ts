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

import { getGridId } from "model/selectors/DataView/getGridId";
import { getEntity } from "model/selectors/DataView/getEntity";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { getDataView } from "model/selectors/DataView/getDataView";
import { shouldProceedToChangeRow } from "./TableView/shouldProceedToChangeRow";
import { getFilterConfiguration } from "model/selectors/DataView/getFilterConfiguration";

export function onCreateRowClick(ctx: any, switchToFormPerspective?: boolean) {
  return flow(function*onCreateRowClick(event: any) {
    try {
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
      const filterConfiguration = getFilterConfiguration(ctx);
      filterConfiguration.clearFilters();
      yield*formScreenLifecycle.onCreateRow(entity, gridId);
    } catch (e) {
      yield*handleError(ctx)(e);
      throw e;
    }
  });
}

