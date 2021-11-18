export interface IFilterSetting {
  type: string;
  val1?: any;
  val2?: any;
  filterValue1: any | undefined;
  filterValue2: any | undefined;
  val1ServerForm: any | undefined;
  val2ServerForm: any | undefined;
  isComplete: boolean;
  lookupId: string | undefined
}