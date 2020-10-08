// TODO: Extract types so that model layer does not depend on view layer?

import { IFilter } from "./IFilter";

export interface IFilterConfigurationData {}

export interface IFilterConfiguration extends IFilterConfigurationData {
  $type_IFilterConfigurationData: 1;

  isFilterControlsDisplayed: boolean;
  filters: any[];
  filteringFunction: () => (row: any[]) => boolean;
  getSettingByPropertyId(propertyId: string): IFilter | undefined;
  setFilter(term: IFilter): void;
  clearFilters(): void;

  onFilterDisplayClick(event: any): void;

  parent?: any;
}
