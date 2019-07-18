import { IFilterConfiguration } from "./types/IFilterConfiguration";
import { observable, action } from "mobx";

export class FilterConfiguration implements IFilterConfiguration {
  $type_IFilterConfigurationData: 1 = 1;  
  
  @observable isFilterControlsDisplayed: boolean = false;
  
  @action.bound
  onFilterDisplayClick(event: any): void {
    this.isFilterControlsDisplayed = !this.isFilterControlsDisplayed;
  }
  
  parent?: any;
}