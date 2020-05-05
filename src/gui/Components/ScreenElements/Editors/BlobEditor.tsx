import React from "react";
import { inject, observer } from "mobx-react";
import { getApi } from "model/selectors/getApi";
import { getDataStructureEntityId } from "model/selectors/DataView/getDataStructureEntityId";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getMenuItemId } from "model/selectors/getMenuItemId";
import { getEntity } from "model/selectors/DataView/getEntity";
import { getSessionId } from "model/selectors/getSessionId";
import { IApi } from "model/entities/types/IApi";
import { IProperty } from "model/entities/types/IProperty";

@inject(({ property }: { property: IProperty }, { value }) => {
  return {
    api: getApi(property),
    DataStructureEntityId: getDataStructureEntityId(property),
    Property: property.id,
    RowId: getSelectedRowId(property),
    menuItemId: getMenuItemId(property),
    Entity: getEntity(property),
    SessionFormIdentifier: getSessionId(property),
    parameters: property.parameters,
  };
})
@observer
export class BlobEditor extends React.Component<{
  value: string;
  api?: IApi;
  DataStructureEntityId?: string;
  Property?: string;
  RowId?: string;
  menuItemId?: string;
  Entity?: string;
  SessionFormIdentifier?: string;
  parameters?: any;
}> {
  async download() {
    console.log(this.props.parameters);
    const token = await this.props.api!.getDownloadToken({
      SessionFormIdentifier: this.props.SessionFormIdentifier!,
      MenuId: this.props.menuItemId!,
      DataStructureEntityId: this.props.DataStructureEntityId!,
      Entity: this.props.Entity!,
      RowId: this.props.RowId!,
      Property: this.props.Property!,
      FileName: this.props.value,
      parameters: this.props.parameters,
    });
    this.props.api!.getBlob({ downloadToken: token });
  }

  render() {
    return (
      <div className="blobEditor">
        <button onClick={() => this.download()}>{this.props.value}</button>
      </div>
    );
  }
}
