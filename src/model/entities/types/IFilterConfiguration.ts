
import { IDataTable } from "./IDataTable";

// TODO: Extract types so that model layer does not depend on view layer?



export interface IFilterConfigurationData {}

export interface IFilterConfiguration extends IFilterConfigurationData {
  $type_IFilterConfigurationData: 1;

  isFilterControlsDisplayed: boolean;
  filters: any[];
  filteringFunction: () => (row: any[]) => boolean ;
  getSettingByPropertyId(propertyId: string): any;
  setFilter(term: any): void;
  clearFilters(): void;

  onFilterDisplayClick(event: any): void;

  parent?: any;
}
