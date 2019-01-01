import { observable } from "mobx";
import { IFieldType, IDataTableFieldStruct, IFieldId, IDropdownColumn } from './types';

export class DataTableField implements IDataTableFieldStruct {
  
  @observable
  public formOrder: number;

  @observable 
  public gridVisible: boolean;
  
  @observable
  public formVisible: boolean;
  
  public type: IFieldType;
  

  @observable
  public label: string;

  @observable
  public dataIndex: number;

  public recvDataIndex: number;

  public id: IFieldId;

  public isPrimaryKey: boolean;
  public isLookedUp: boolean;

  public lookupId: string;
  public lookupIdentifier: string;
  public dropdownColumns: IDropdownColumn[] = [];

  constructor({
    id,
    label,
    type,
    dataIndex,
    recvDataIndex,
    isPrimaryKey,
    isLookedUp,
    lookupId,
    lookupIdentifier,
    dropdownColumns
  }: {
    id: IFieldId;
    label: string;
    type: IFieldType;
    dataIndex: number;
    recvDataIndex: number;
    isPrimaryKey: boolean;
    isLookedUp: boolean;
    lookupId?: string;
    lookupIdentifier?: string;
    dropdownColumns?: IDropdownColumn[];
  }) {
    Object.assign(this, {
      id,
      label,
      type,
      dataIndex,
      recvDataIndex,
      isPrimaryKey,
      isLookedUp,
      lookupId,
      lookupIdentifier,
      dropdownColumns
    });
  }
}