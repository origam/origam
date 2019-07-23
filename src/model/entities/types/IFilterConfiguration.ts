export interface IFilterConfigurationData {

}

export interface IFilterConfiguration extends IFilterConfigurationData {
  $type_IFilterConfigurationData: 1;
  
  isFilterControlsDisplayed: boolean;

  onFilterDisplayClick(event: any): void;

  parent?: any;
}