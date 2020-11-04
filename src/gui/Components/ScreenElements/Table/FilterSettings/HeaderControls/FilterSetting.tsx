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

  get val1ServerForm(){
    return this.val1;
  }

  get val2ServerForm(){
    return this.val2;
  }

  constructor(type: string, isComplete:boolean=false, val1?: any, val2?: any) {
    this.type = type;
    this.isComplete = isComplete;
    this.val1 = val1 ?? undefined;
    this.val2 = val2 ?? undefined;
  }
}
