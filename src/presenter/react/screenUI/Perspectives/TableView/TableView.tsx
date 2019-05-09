import * as React from "react";
import { Table } from "./Table/Table";
import { observer } from "mobx-react";
import { Toolbar } from "../DefaultToolbar/Toolbar";
import { ITableView as ITableViewModel } from "../../../../../DataView/TableView/ITableView";

import { TableViewPresenter } from "../../../../view/Perspectives/TableView/TableViewPresenter";
import { TableViewToolbar } from "../../../../view/Perspectives/TableView/TableViewToolbar";
import { TableViewTable } from "../../../../view/Perspectives/TableView/TableViewTable";
import { TableViewScrollState } from "../../../../view/Perspectives/TableView/TableViewScrollState";
import { TableViewCells } from "../../../../view/Perspectives/TableView/TableViewCells";
import { ITableView } from "../../../../view/Perspectives/TableView/types";
import { TableViewField } from "../../../../view/Perspectives/TableView/TableViewField";

@observer
export class TableView extends React.Component<{
  controller: ITableViewModel;
}> {
  constructor(props: any) {
    super(props);
    this.tableViewPresenter = new TableViewPresenter({
      toolbar: () => tableViewToolbar,
      table: () => tableViewTable
    });
    const tableViewToolbar = this.props.controller.dataView.isHeadless
      ? undefined
      : new TableViewToolbar({
          dataTable: () => this.props.controller.dataView.dataTable,
          aSwitchView: () => this.props.controller.dataView.aSwitchView,
          mediator: () => this.props.controller.dataView.mediator,
          label: this.props.controller.dataView.label,
          isLoading: () => this.props.controller.dataView.machine.isLoading
        });
    const tableViewTable = new TableViewTable({
      scrollState: () => tableViewScrollState,
      cells: () => tableViewCells,
      cursor: () => tableViewField,
      mediator: () => this.props.controller.dataView.mediator,
      editing: () => this.props.controller.dataView.editing,
      aFinishEditing: () => this.props.controller.dataView.aFinishEditing,
      isLoading: () => this.props.controller.dataView.machine.isLoading
    });
    const tableViewCells = new TableViewCells({
      dataTable: this.props.controller.dataView.dataTable,
      propReorder: () => this.props.controller.propReorder,
      recCursor: () => this.props.controller.recCursor,
      propCursor: () => this.props.controller.propCursor
    });
    const tableViewField = new TableViewField({
      dataTable: this.props.controller.dataView.dataTable,
      propReorder: () => this.props.controller.propReorder,
      recCursor: () => this.props.controller.recCursor,
      propCursor: () => this.props.controller.propCursor,
      editing: () => this.props.controller.dataView.editing,
      form: () => this.props.controller.dataView.form
    });
    const tableViewScrollState = new TableViewScrollState(0, 0);
  }

  tableViewPresenter: ITableView;

  render() {
    return (
      <div className="table-view">
        {this.tableViewPresenter.toolbar && (
          <Toolbar controller={this.tableViewPresenter.toolbar} />
        )}
        <Table controller={this.tableViewPresenter.table} />
      </div>
    );
  }
}
