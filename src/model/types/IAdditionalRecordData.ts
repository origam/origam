export const CAdditionalRowData = "CAdditionalRowData";

export interface IAdditionalRowDataData {

}

export interface IAdditionalRowData extends IAdditionalRowDataData {
  $type: typeof CAdditionalRowData;

  dirtyNew: boolean;
  dirtyDeleted: boolean;
  dirtyValues: Map<string, any>;
  dirtyFormValues: Map<string, any>;

  


  parent?: any;
}                                                                                                 












