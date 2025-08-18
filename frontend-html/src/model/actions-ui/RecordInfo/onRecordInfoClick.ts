/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { flow } from "mobx";
import { getMenuItemId } from "model/selectors/getMenuItemId";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getDataStructureEntityId } from "model/selectors/DataView/getDataStructureEntityId";
import { getRecordInfo } from "model/selectors/RecordInfo/getRecordInfo";
import { getSessionId } from "model/selectors/getSessionId";

export function onRecordInfoClick(ctx: any) {
  return flow(function*onRecordInfoClick(event: any) {
    const menuId = getMenuItemId(ctx);
    const dataStructureEntityId = getDataStructureEntityId(ctx);
    const rowId = getSelectedRowId(ctx);
    const sessionId = getSessionId(ctx);
    if (rowId) {
      yield*getRecordInfo(ctx).onOpenRecordInfoClick(
        event,
        menuId,
        dataStructureEntityId,
        rowId,
        sessionId,
      );
    }
  });
}
