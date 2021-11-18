export interface IActionParameterData {
  name: string;
  fieldName: string;
}

export interface IActionParameter extends IActionParameterData {
  $type_IActionParameter: 1;

  parent?: any;
}