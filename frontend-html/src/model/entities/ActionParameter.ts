import {IActionParameter, IActionParameterData} from "./types/IActionParameter";

export class ActionParameter implements IActionParameter {
  $type_IActionParameter: 1 = 1;

  constructor(data: IActionParameterData) {
    Object.assign(this, data);
  }

  name: string = "";
  fieldName: string = "";

  parent?: any;
}
