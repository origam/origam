import * as React from "react";
import { observer } from "mobx-react";
import { DataTableField } from "src/DataTable/DataTableState";
import { IFieldType } from "src/DataTable/types";
import { GridTable } from "../controls/GridTable";
import { GridToolbar } from "../controls/GridToolbar";

const personFields = [
  new DataTableField({
    id: "name",
    label: "Name",
    type: IFieldType.string,
    dataIndex: 0,
    isLookedUp: false
  }),
  new DataTableField({
    id: "birth_date",
    label: "Birth date",
    type: IFieldType.date,
    dataIndex: 1,
    isLookedUp: false
  }),
  new DataTableField({
    id: "likes_platypuses",
    label: "Likes platypuses?",
    type: IFieldType.boolean,
    dataIndex: 2,
    isLookedUp: false
  }),
  new DataTableField({
    id: "city_id",
    label: "Lives in",
    type: IFieldType.string,
    dataIndex: 3,
    isLookedUp: true,
    lookupResultFieldId: "name",
    lookupResultTableId: "city"
  }),
  new DataTableField({
    id: "favorite_color",
    label: "Favorite color",
    type: IFieldType.color,
    dataIndex: 4,
    isLookedUp: false
  })
];

const cityFields = [
  new DataTableField({
    id: "name",
    label: "Name",
    type: IFieldType.string,
    dataIndex: 0,
    isLookedUp: false
  }),
  new DataTableField({
    id: "inhabitants",
    label: "Inhabitants",
    type: IFieldType.integer,
    dataIndex: 1,
    isLookedUp: false
  })
];

@observer
export class Grid extends React.Component<any> {
  public render() {
    return (
      <div
        className="oui-grid"
        style={{
          maxWidth: this.props.w,
          maxHeight: this.props.h
        }}
      >
        <GridToolbar
          name={this.props.name}
          isHidden={this.props.isHeadless}
          isAddButton={this.props.isShowAddButton}
          isDeleteButton={this.props.isShowDeleteButton}
          isCopyButton={this.props.isShowAddButton}
        />
        {
          <GridTable
            initialDataTableName="person"
            initialFields={personFields}
          />
        }
        {/*this.props.children*/}
      </div>
    );
  }
}
