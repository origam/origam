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

import { getRowStateIsDisableAction } from "model/selectors/RowState/getRowStateIsDisabledAction";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { IAction, IActionData, IActionMode, IActionPlacement, IActionType } from "./types/IAction";
import { IActionParameter } from "./types/IActionParameter";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getRowStateById } from "model/selectors/RowState/getRowStateById";

export class Action implements IAction {
  $type_IAction: 1 = 1;

  constructor(data: IActionData) {
    Object.assign(this, data);
    this.parameters.forEach((o) => (o.parent = this));
  }

  type: IActionType = null as any;
  id: string = "";
  groupId: string = "";
  caption: string = "";
  placement: IActionPlacement = null as any;
  iconUrl: string = "";
  mode: IActionMode = null as any;
  isDefault: boolean = false;
  parameters: IActionParameter[] = [];
  showAlways: boolean = false;

  get isEnabled() {
    if (this.mode === IActionMode.Always || this.showAlways) {
      return true;
    }

    switch (this.mode) {
      case IActionMode.ActiveRecord: {
        const selRowId = getSelectedRowId(this);
        return selRowId ? !getRowStateIsDisableAction(this, selRowId, this.id) : false;
      }
      case IActionMode.MultipleCheckboxes: {
        const selectedIds = getDataView(this).selectedRowIds;
        return selectedIds.size > 0
          ? !Array.from(selectedIds)
            .map((rowId) => IsDisabledMultipleCheckboxAction(this, rowId, this.id))
            .some((item) => item)
          : this.placement === IActionPlacement.Toolbar;
      }
    }
    return false;
  }

  parent?: any;
}

export function IsDisabledMultipleCheckboxAction(ctx: any, rowId: string, actionId: string) {
  const rowState = getRowStateById(ctx, rowId);
  return rowState && rowState.disabledActions
    ? rowState.disabledActions.has(actionId)
    : false;
}
