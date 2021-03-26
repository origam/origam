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

@inject(({ property }: { property: IProperty }, { value }) => {
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
    return preparedUrl ? <img className={S.image} src={preparedUrl}  alt=""/> : null;
  }
}
