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
import "./index.scss";
import { useRootStore } from "./components/Common";
import { HashtagDialogContent } from "./components/HashtagDialog";
import {
  Column,
  Column2TouchMoveControlee,
  DataSource,
  DataSourceField,
  DataTable,
  IColumn,
  IDataSourceField,
} from "./stores/DataTableStore";
import { HashtagRootStore } from "./stores/RootStore";
import { ObjectTouchMover } from "./util/ObjectTouchMover";
import { TableCursor } from "./stores/TableCursorStore";
import {
  DefaultModal,
  ModalCloseButton,
  ModalFooter,
} from "../../components/Windows/Windows";
import { Button } from "../../components/Buttons";
import { observer } from "mobx-react";
import { T, TR } from "util/translation";

export function capitalize(sin: string) {
  return sin.charAt(0).toUpperCase() + sin.slice(1);
}

export function populateHashtaggingStore(rootStore: HashtagRootStore) {
  function makeObjectsTable() {
    const dataTable = new DataTable(new TableCursor(), "objects", 0);
    const columns: IColumn[] = [];

    for (let c of columns) {
      c.touchMover = new ObjectTouchMover(new Column2TouchMoveControlee(c));
    }

    dataTable.columns.push(...columns);
    const dataSource = new DataSource("objects");
    const dataSourceFields: IDataSourceField[] = [];
    dataSource.fields.push(...dataSourceFields);
    dataTable.dataSource = dataSource;

    /*for (let i = 0; i < 1000; i++) {
    dataTable.rows.push([
      `id-obj-${i}`,
      faker.name.firstName(),
      faker.name.lastName(),
      faker.address.city(),
      faker.date.past().toString(),
      faker.random.number(500),
    ]);
  }*/

    return dataTable;
  }

  const dtObjects = makeObjectsTable();

  function makeCategoriesTable() {
    const dataTable = new DataTable(new TableCursor(), "categories", 0);
    const columns = [
      new Column(dataTable, "deepLinkLabel", TR("Name", "name"), "text", ""),
    ];

    columns[0].touchMover = new ObjectTouchMover(
      new Column2TouchMoveControlee(columns[0])
    );

    dataTable.columns.push(...columns);
    const dataSource = new DataSource("categories");
    const dataSourceFields = [
      new DataSourceField("deepLinkName", 0),
      new DataSourceField("deepLinkLabel", 1),
      new DataSourceField("objectComboboxMetadata", 2),
    ];
    dataSource.fields.push(...dataSourceFields);
    dataTable.dataSource = dataSource;
    /*for (let i = 0; i < 50; i++) {
    dataTable.rows.push([`id-cat-${i}`, capitalize(faker.random.word())]);
  }*/
    return dataTable;
  }

  const dtCategories = makeCategoriesTable();

  rootStore.dataTableStore.dataTables.push(dtObjects, dtCategories);
}

export function renderHashtaggingDialog() {
  return <Dialog />;
}

const Dialog = observer(function Dialog() {
  const root = useRootStore();
  const selectedIds = root.dataTableStore.getDataTable("objects")
    ?.selectedRowIds;
  const selectedCount = selectedIds ? selectedIds.size : 0;
  return (
    <DefaultModal
      footer={
        <ModalFooter align="center">
          {selectedCount > 0 && (
            <Button onClick={root.screenProcess.handleOkClick}>
              {T("Create tags", "create_tags")} ({selectedCount})
            </Button>
          )}
          <Button onClick={root.screenProcess.handleCancelClick}>
            {T("Cancel", "Cancel")}
          </Button>
        </ModalFooter>
      }
    >
      <ModalCloseButton onClick={root.screenProcess.handleCloseClick} />
      <div className="hashtaggingDialogContainer">
        <HashtagDialogContent />
      </div>
    </DefaultModal>
  );
});
