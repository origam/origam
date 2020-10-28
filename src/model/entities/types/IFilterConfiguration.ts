// TODO: Extract types so that model layer does not depend on view layer?

import { IFilter } from "./IFilter";
import {IFilterGroup} from "model/entities/types/IFilterGroup";

export interface IFilterConfigurationData {}

export interface IFilterConfiguration extends IFilterConfigurationData {
  $type_IFilterConfigurationData: 1;

  isFilterControlsDisplayed: boolean;
  filterGroups: IFilterGroup[];
  defaultFilter: IFilterGroup | undefined;
  activeFilters: any[];
  filteringFunction: () => (row: any[]) => boolean;
  getSettingByPropertyId(propertyId: string): IFilter | undefined;
  setFilter(term: IFilter): void;
  setFilterGroup(filterGroup: IFilterGroup | undefined): void;
  clearFilters(): void;

  onFilterDisplayClick(event: any): void;

  parent?: any;
}
