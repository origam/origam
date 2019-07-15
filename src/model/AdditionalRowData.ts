import {
  IAdditionalRowData,
} from "./types/IAdditionalRecordData";
import { observable } from "mobx";

export class AdditionalRowData implements IAdditionalRowData {
  $type_IAdditionalRowData: 1 = 1;

  @observable dirtyNew: boolean = false;
  @observable dirtyDeleted: boolean = false;
  @observable dirtyValues: Map<string, any> = new Map();
  @observable dirtyFormValues: Map<string, any> = new Map();

  parent?: any;
}
