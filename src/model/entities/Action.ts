import { computed } from "mobx";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { getRowStateIsDisableAction } from "model/selectors/RowState/getRowStateIsDisabledAction";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import {
  IAction,
  IActionData,
  IActionMode,
  IActionPlacement,
  IActionType,
} from "./types/IAction";
import { IActionParameter } from "./types/IActionParameter";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getDataView } from "model/selectors/DataView/getDataView";

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

  @computed get isEnabled() {
    if (this.mode === IActionMode.Always) {
      return true;
    }
  
    switch (this.mode) {
      case IActionMode.ActiveRecord: {
        const selRowId = getSelectedRowId(this);
        return selRowId 
          ? !getRowStateIsDisableAction(this, selRowId, this.id) 
          : false;
      }
      case IActionMode.MultipleCheckboxes: {
        const selectedIds = getDataView(this).selectedRowIds;
          return selectedIds.size > 0
            ? !Array.from(selectedIds).some(rowId => getRowStateIsDisableAction(this, rowId, this.id))
            : this.placement === IActionPlacement.Toolbar;
      }
    }
  }

  parent?: any;
}
