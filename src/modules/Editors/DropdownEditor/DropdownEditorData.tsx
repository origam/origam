import { bind } from "@decorize/bind";
import { action, computed } from "mobx";
import { DataViewData } from "modules/DataView/DataViewData";
import { RowCursor } from "modules/DataView/TableCursor";
import { DropdownEditorSetup } from "./DropdownEditor";

export interface IDropdownEditorData {
  idsInEditor: string[];
  value: string | string[] | null;
  text: string;
  isResolving: boolean;
  chooseNewValue(value: any): void;
  remove(value: any): void;
}

@bind
export class DropdownEditorData implements IDropdownEditorData {
  constructor(
    private dataTable: DataViewData,
    private rowCursor: RowCursor,
    private setup: () => DropdownEditorSetup
  ) {}

  @computed get value(): string | string[] | null {
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

  idsInEditor: string[] = [];

  remove(value: any): void {
    // not needed in DropdownEditorData
  }
}
