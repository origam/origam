import { flow } from "mobx";
import { getDataView } from "model/selectors/DataView/getDataView";
import { IProperty } from "model/entities/types/IProperty";

export function onDropdownEditorClick(ctx: any) {
  return flow(function* onDropdownEditorClick(
    event: any,
    property: IProperty | undefined,
    currentRow: any[] | undefined
  ) {
    if (currentRow && property?.isLink && (event.ctrlKey || event.metaKey)) {
      yield* getDataView(ctx).navigateLookupLink(property, currentRow);
    }
  });
}
