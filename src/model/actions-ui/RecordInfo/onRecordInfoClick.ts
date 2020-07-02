import {flow} from "mobx";
import {getMenuItemId} from "model/selectors/getMenuItemId";
import {getSelectedRowId} from "model/selectors/TablePanelView/getSelectedRowId";
import {getDataStructureEntityId} from "model/selectors/DataView/getDataStructureEntityId";
import {getRecordInfo} from "model/selectors/RecordInfo/getRecordInfo";

export function onRecordInfoClick(ctx: any) {
  return flow(function* onRecordInfoClick(event: any) {
    const menuId = getMenuItemId(ctx);
    const dataStructureEntityId = getDataStructureEntityId(ctx);
    const rowId = getSelectedRowId(ctx);
    if (rowId) {
      yield* getRecordInfo(ctx).onOpenRecordInfoClick(
        event,
        menuId,
        dataStructureEntityId,
        rowId
      );
    }
  });
}
