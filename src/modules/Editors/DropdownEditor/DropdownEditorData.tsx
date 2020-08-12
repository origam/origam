import { bind } from "@decorize/bind";
import { TypeSymbol } from "dic/Container";
import { action, computed } from "mobx";
import { DataViewData } from "modules/DataView/DataViewData";
import { RowCursor } from "modules/DataView/TableCursor";
import { DropdownEditorSetup } from "./DropdownEditor";

export interface IDropdownEditorData {
  value: any | null;
  text: string;
  isResolving: boolean;
  chooseNewValue(value: any): void;
}

@bind
export class DropdownEditorData implements IDropdownEditorData {
  constructor(
    private dataTable: DataViewData,
    private rowCursor: RowCursor,
    private setup: () => DropdownEditorSetup
  ) {}

  @computed get value(): any | null {
    if (this.rowCursor.selectedId) {
      return this.dataTable.getCellValue(this.rowCursor.selectedId, this.setup().propertyId);
    } else return null;
  }

  @computed get text(): string {
    if (!this.isResolving && this.value) {
      return this.dataTable.getCellText(this.setup().propertyId, this.value);
    } else return "";
  }

  get isResolving() {
    return this.dataTable.getIsCellTextLoading(this.setup().propertyId, this.value);
  }

  @action.bound chooseNewValue(value: any) {
    if (this.rowCursor.selectedId) {
      this.dataTable.setNewValue(this.rowCursor.selectedId, this.setup().propertyId, value);
    }
  }
}
// export const IDropdownEditorData = TypeSymbol<DropdownEditorData>("IDropdownEditorData");

@bind
export class TagInputEditorData implements IDropdownEditorData {
  constructor(
    private dataTable: DataViewData,
    private rowCursor: RowCursor,
    private setup: () => DropdownEditorSetup,
    private onChange?: (event: any, value: any)=>void
  ) {}

  @computed get value(): any | null {
    if (this.rowCursor.selectedId) {
      return this.dataTable.getCellValue(this.rowCursor.selectedId, this.setup().propertyId);
    } else return null;
  }

  @computed get text(): string {
    if (!this.isResolving && this.value) {
      return this.dataTable.getCellText(this.setup().propertyId, this.value);
    } else return "";
  }

  get isResolving() {
    return this.dataTable.getIsCellTextLoading(this.setup().propertyId, this.value);
  }

  @action.bound chooseNewValue(value: any) {
    const newArray =[...this.value, value];
    if (this.rowCursor.selectedId) {
      this.dataTable.setNewValue(this.rowCursor.selectedId, this.setup().propertyId, newArray);
    }
    // this.onChange && this.onChange(null, newArray);
  }
}
