import {
  IAction,
  IActionData,
  IActionType,
  IActionPlacement,
  IActionMode
} from "./types/IAction";
import { IActionParameter } from "./types/IActionParameter";

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

  parent?: any;
}
