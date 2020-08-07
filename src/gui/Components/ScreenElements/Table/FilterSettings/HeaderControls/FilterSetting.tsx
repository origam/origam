import { IFilterSetting } from "../../../../../../model/entities/types/IFilterSetting";

export class FilterSetting implements IFilterSetting {
  type: string;
  caption: string;
  val1?: any;
  val2?: any;
  isComplete: boolean;

  get filterValue1() {
    return this.val1;
  }

  get filterValue2() {
    return this.val2;
  }

  constructor(type: string, caption: string) {
    this.type = type;
    this.caption = caption;
    this.isComplete = false;
  }
}