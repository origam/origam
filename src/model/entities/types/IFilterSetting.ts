export interface IFilterSetting {
  type: string;
  caption: JSX.Element;
  val1?: any;
  val2?: any;
  filterValue1: any | undefined;
  filterValue2: any | undefined;
  isComplete: boolean;
  lookupId: string | undefined
}