import {
  IAction,
  IActionData,
  IActionType,
  IActionPlacement,
  IActionMode
} from "./types/IAction";
import { IActionParameter } from "./types/IActionParameter";
import { computed } from "mobx";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";

export class Action implements IAction {
  $type_IAction: 1 = 1;

  constructor(data: IActionData) {
    Object.assign(this, data);
    this.parameters.forEach(o => (o.parent = this));
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
    switch (this.mode) {
      case IActionMode.Always: {
        return true;
      }
      case IActionMode.ActiveRecord: {
        const selectedRow = getSelectedRow(this);
        return !!selectedRow;
      }
      case IActionMode.MultipleCheckboxes: {
        // TODO: Multiple checkboxes case
        return false;
      }
    }
  }

  parent?: any;
}
