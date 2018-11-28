import { observable, action } from "mobx";

export class Filter {
  @observable public implicitFilter: any = [];
  
  public get generatedFilter(): any {
    return this.implicitFilter;
  }

  @action.bound public setImplicitFilter(filter: any) {
    this.implicitFilter = filter;
  }
}