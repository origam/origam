import {flow} from "mobx";
import {getMenuItemId} from "model/selectors/getMenuItemId";
import {getDataStructureEntityId} from "model/selectors/DataView/getDataStructureEntityId";
import {getSelectedRowId} from "model/selectors/TablePanelView/getSelectedRowId";
import {getRecordInfo} from "model/selectors/RecordInfo/getRecordInfo";

export function onRecordAuditClick(ctx: any) {
  return flow(function* onRecordAuditClick(event: any) {
    const menuId = getMenuItemId(ctx);
    const dataStructureEntityId = getDataStructureEntityId(ctx);
    const rowId = getSelectedRowId(ctx);
    if (rowId) {
      yield* getRecordInfo(ctx).onOpenRecordAuditClick(
        event,
        menuId,
        dataStructureEntityId,
        rowId
      );
    }
  })
}