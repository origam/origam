import { IPropertyData, IProperty, IDockType } from "./types/IProperty";
import { ICaptionPosition } from "./types/ICaptionPosition";
import { IDropDownColumn } from "./types/IDropDownColumn";
import { IPropertyColumn } from "./types/IPropertyColumn";
import { observable, computed, action } from "mobx";

import { ILookup } from "./types/ILookup";
import { getDataSourceFieldIndexByName } from "../selectors/DataSources/getDataSourceFieldIndexByName";
import { getDataSourceFieldByName } from "model/selectors/DataSources/getDataSourceFieldByName";
import { IDataSource } from "./types/IDataSource";
import { IDataSourceField } from "./types/IDataSourceField";

export class Property implements IProperty {
  $type_IProperty: 1 = 1;

  constructor(data: IPropertyData) {
    Object.assign(this, data);
    if (this.lookup) {
      this.lookup.parent = this;
    }
  }

  id: string = "";
  modelInstanceId: string = "";
  name: string = "";
  nameOverride: string | null | undefined = null;
  readOnly: boolean = false;
  x: number = 0;
  y: number = 0;
  width: number = 0;
  height: number = 0;
  captionLength: number = 0;
  captionPosition?: ICaptionPosition;
  entity: string = "";
  column: IPropertyColumn = IPropertyColumn.Text;
  dock?: IDockType | undefined;
  multiline: boolean = false;
  isPassword: boolean = false;
  isRichText: boolean = false;
  maxLength: number = 0;
  allowReturnToForm?: boolean | undefined;
  isTree?: boolean | undefined;
  formatterPattern: string = "";
  customNumericFormat: string = "";
  gridColumnWidth: number = 100;
  @observable columnWidth: number = 100;
  identifier?: string;
  lookup?: ILookup;
  isAggregatedColumn: boolean = false;

  linkToMenuId?: string = undefined;

  get isLookup() {
    return !!this.lookup;
  }

  get isLink() {
    return !!this.linkToMenuId;
  }

  @action.bound setColumnWidth(width: number) {
    this.columnWidth = width;
  }

  @computed get dataSourceIndex(): number {
    return this.dataSourceField.index;
  }

  @computed get dataIndex() {
    return this.dataSourceIndex;
  }

  @computed get dataSourceField(): IDataSourceField {
    return getDataSourceFieldByName(this, this.id)!;
  }

  parent: any;
}
