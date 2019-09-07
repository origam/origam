import { ISetting as IFilterSettingString } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/FilterSettingsString";
import { ISetting as IFilterSettingDate } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/FilterSettingsDate";
import { ISetting as IFilterSettingNumber } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/FilterSettingsNumber";
import { IDataTable } from "./IDataTable";

// TODO: Extract types so that model layer does not depend on view layer?

type ISetting =
  | IFilterSettingString
  | IFilterSettingDate
  | IFilterSettingNumber;

export interface IFilterTerm {
  propertyId: string;
  setting: ISetting;
}

export interface IFilterConfigurationData {}

export interface IFilterConfiguration extends IFilterConfigurationData {
  $type_IFilterConfigurationData: 1;

  isFilterControlsDisplayed: boolean;
  filtering: IFilterTerm[];
  filteringFunction: (dataTable: IDataTable) => (row: any[]) => boolean;
  getSettingByPropertyId(propertyId: string): IFilterTerm | undefined;
  setFilter(term: IFilterTerm): void;
  clearFilters(): void;

  onFilterDisplayClick(event: any): void;

  parent?: any;
}
