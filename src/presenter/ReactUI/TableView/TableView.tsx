import * as React from "react";
import { Table } from "./Table/Table";
import { observer } from "mobx-react";
import { Toolbar } from "../controls/Toolbar/Toolbar";
import { ITableView } from "src/presenter/types/ITableViewPresenter/ITableView";

@observer
export class TableView extends React.Component<{ controller: ITableView }> {
  render() {
    return (
      <div className="table-view">
        {this.props.controller.toolbar && (
          <Toolbar controller={this.props.controller.toolbar} />
        )}
        <Table controller={this.props.controller.table} />
      </div>
    );
  }
}
