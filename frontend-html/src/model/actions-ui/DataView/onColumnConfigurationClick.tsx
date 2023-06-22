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

import { getColumnConfigurationModel } from "model/selectors/getColumnConfigurationModel";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { shouldProceedToChangeRow } from "./TableView/shouldProceedToChangeRow";
import { getDialogStack } from "model/selectors/DialogStack/getDialogStack";
import { ColumnsDialog } from "gui/Components/Dialogs/ColumnsDialog";
import React from "react";
import { dialogKey } from "model/entities/TablePanelView/ColumnConfigurationModel";

export function onColumnConfigurationClick(ctx: any) {
  return flow(function*onColumnConfigurationClick(event: any) {
    try {
      if (yield shouldProceedToChangeRow(ctx)) {
        const columnConfigModel = getColumnConfigurationModel(ctx);
        columnConfigModel.reset();
        getDialogStack(ctx).pushDialog(
          dialogKey,
          <ColumnsDialog
            model={columnConfigModel}
          />
        );
      }
    } catch (e) {
      yield*handleError(ctx)(e);
      throw e;
    }
  });
}
