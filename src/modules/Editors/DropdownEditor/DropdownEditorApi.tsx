import { bind } from "@decorize/bind";
import { TypeSymbol } from "dic/Container";
import { DataViewAPI } from "modules/DataView/DataViewAPI";
import { RowCursor } from "modules/DataView/TableCursor";
import { DropdownEditorSetup } from "./DropdownEditor";
import { DropdownEditorBehavior } from "./DropdownEditorBehavior";
import { EagerlyLoadedGrid } from "./DropdownEditorCommon";

@bind
export class DropdownEditorApi {
  constructor(
    private setup: () => DropdownEditorSetup,
    private rowCursor: RowCursor,
    private api: DataViewAPI,
    private behavior:() => DropdownEditorBehavior,
  ) {}

  *getLookupList(searchTerm: string): any {
    const setup = this.setup();
    if (setup.dropdownType === EagerlyLoadedGrid) {
      return yield* this.api.getLookupList({
        ColumnNames: setup.columnNames,
        Property: setup.propertyId,
        Id: this.rowCursor.selectedId!,
        LookupId: setup.lookupId,
        Parameters: setup.parameters,
        ShowUniqueValues: setup.showUniqueValues,
        SearchText: searchTerm || "",
        PageSize: -1,
        PageNumber: 1,
      });
    } else {
      return yield* this.api.getLookupList({
        ColumnNames: setup.columnNames,
        Property: setup.propertyId,
        Id: this.rowCursor.selectedId!,
        LookupId: setup.lookupId,
        Parameters: setup.parameters,
        ShowUniqueValues: setup.showUniqueValues,
        SearchText: searchTerm || "",
        PageSize: this.behavior().pageSize,
        PageNumber: this.behavior().willLoadPage,
      });
    }
  }
}
export const IDropdownEditorApi = TypeSymbol<DropdownEditorApi>("IDropdownEditorApi");
