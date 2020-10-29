// TODO: Extract types so that model layer does not depend on view layer?

import { IFilter } from "./IFilter";
import {IFilterGroup} from "model/entities/types/IFilterGroup";

export interface IFilterConfigurationData {}

export interface IFilterConfiguration extends IFilterConfigurationData {
  $type_IFilterConfigurationData: 1;

  isFilterControlsDisplayed: boolean;
  activeFilters: IFilter[];
  filteringFunction: () => (row: any[]) => boolean;
  getSettingByPropertyId(propertyId: string): IFilter | undefined;
  setFilter(term: IFilter): void;
  setFilters(filters: IFilter[]): void;
  clearFilters(): void;

  onFilterDisplayClick(event: any): void;

  parent?: any;
}
