import {
  IFilterConfiguration,
  IFilterTerm
} from "./types/IFilterConfiguration";
import { observable, action } from "mobx";

export class FilterConfiguration implements IFilterConfiguration {
  $type_IFilterConfigurationData: 1 = 1;

  @observable filterSetting: IFilterTerm[] = [];

  getSettingByPropertyId(propertyId: string): IFilterTerm | undefined {
    return this.filterSetting.find(item => item.propertyId === propertyId);
  }

  @action.bound
  setFilter(term: IFilterTerm): void {
    const oldIdx = this.filterSetting.findIndex(
      item => item.propertyId === term.propertyId
    );
    if (oldIdx > -1) {
      this.filterSetting.splice(oldIdx, 1);
    }
    this.filterSetting.push(term);
  }

  @action.bound
  clearFilters(): void {
    this.filterSetting.length = 0;
  }

  @observable isFilterControlsDisplayed: boolean = false;

  @action.bound
  onFilterDisplayClick(event: any): void {
    this.isFilterControlsDisplayed = !this.isFilterControlsDisplayed;
  }

  parent?: any;
}
