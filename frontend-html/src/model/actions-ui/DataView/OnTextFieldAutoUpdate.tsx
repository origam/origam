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

import { IProperty } from "../../entities/types/IProperty";
import { getDataView } from "../../selectors/DataView/getDataView";
import { getSessionId } from "../../selectors/getSessionId";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";

export async function onTextFieldAutoUpdate(property: IProperty, value: string) {
    const dataView = getDataView(property);
    if (!dataView.selectedRowId) {
        return;
    }
    const sessionId = getSessionId(dataView);
    const valueObj = {} as any;
    valueObj[property.id] = value;

    const updateData = [
        {
            RowId: dataView.selectedRowId,
            Values: valueObj,
        }
    ];
    const formScreenLifecycle = getFormScreenLifecycle(property);
    await formScreenLifecycle.updateRequestAggregator.enqueue({
        SessionFormIdentifier: sessionId,
        Entity: dataView.entity,
        UpdateData: updateData,
    });
}