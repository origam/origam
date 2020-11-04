import { bind } from "@decorize/bind";
import { DataViewData } from "../../DataView/DataViewData";
import { RowCursor } from "../../DataView/TableCursor";
import { DropdownEditorSetup } from "./DropdownEditor";
import { action, computed } from "mobx";
import { DropdownEditorData, IDropdownEditorData } from "./DropdownEditorData";

@bind
export class TagInputEditorData implements IDropdownEditorData {
  dropdownEditorData: IDropdownEditorData;

  constructor(
    private dataTable: DataViewData,
    private rowCursor: RowCursor,
    private setup: () => DropdownEditorSetup
  ) {
    this.dropdownEditorData = new DropdownEditorData(dataTable, rowCursor, setup);
  }

  @computed get value(): string | string[] | null {
    return this.dropdownEditorData.value;
  }

  @computed get text(): string {
    return this.dropdownEditorData.text;
  }

  get isResolving() {
    return this.dropdownEditorData.isResolving;
  }

  @action.bound chooseNewValue(value: any) {
    const newArray = [...this.value, value];
    if (this.rowCursor.selectedId) {
      this.dataTable.setNewValue(this.rowCursor.selectedId, this.setup().propertyId, newArray);
    }
  }

  get idsInEditor() {
    return (this.value ? this.value : []) as string[];
  }

  remove(value: any): void {
    // not needed in TagInputEditorData
  }
}
