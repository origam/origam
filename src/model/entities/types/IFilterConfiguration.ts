export interface ILTOp {
  type: "LT";
  propertyId: string;
  val1: any;
}

export interface IGTOp {
  type: "GT";
  propertyId: string;
  val1: any;
}

export interface IIsNull {
  type: "IsNull";
  propertyId: string;
}

export type IFilterTerm = ILTOp | IGTOp | IIsNull;



export interface IFilterConfigurationData {

}

export interface IFilterConfiguration extends IFilterConfigurationData {
  $type_IFilterConfigurationData: 1;
  
  isFilterControlsDisplayed: boolean;
  filterSetting: IFilterTerm[];
  getSettingByPropertyId(propertyId: string): IFilterTerm | undefined;
  setFilter(term: IFilterTerm): void;
  clearFilters(): void;

  onFilterDisplayClick(event: any): void;

  parent?: any;
}