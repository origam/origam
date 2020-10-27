import { IFilterSetting } from "../../../../../../model/entities/types/IFilterSetting";

export class FilterSetting implements IFilterSetting {
  type: string;
  val1?: any;
  val2?: any;
  isComplete: boolean;
  lookupId: string | undefined;

  get filterValue1() {
    return this.val1;
  }

  get filterValue2() {
    return this.val2;
  }

  constructor(type: string) {
    this.type = type;
    this.isComplete = false;
  }
}
