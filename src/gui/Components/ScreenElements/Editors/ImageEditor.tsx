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

import React from "react";
import { inject, observer } from "mobx-react";
import { IProperty } from "model/entities/types/IProperty";
import { getSessionId } from "model/selectors/getSessionId";
import { getEntity } from "model/selectors/DataView/getEntity";
import { getMenuItemId } from "model/selectors/getMenuItemId";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getDataStructureEntityId } from "model/selectors/DataView/getDataStructureEntityId";
import { getApi } from "model/selectors/getApi";
import S from "./ImageEditor.module.scss";
import { processedImageURL } from "utils/image";

@inject(({property}: { property: IProperty }, {value}) => {
  return {
    api: getApi(property),
    DataStructureEntityId: getDataStructureEntityId(property),
    Property: property.id,
    RowId: getSelectedRowId(property),
    Identifier: property.identifier,
    MenuId: getMenuItemId(property),
    Entity: getEntity(property),
    SessionFormIdentifier: getSessionId(property),
  };
})
@observer
export class ImageEditor extends React.Component<{
  value: string;
}> {
  render() {
    const preparedUrl = processedImageURL(this.props.value).value;
    return preparedUrl ? <img className={S.image} src={preparedUrl} alt=""/> : null;
  }
}
