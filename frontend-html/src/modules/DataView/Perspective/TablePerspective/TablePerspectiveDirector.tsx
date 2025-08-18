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
import { action, flow } from "mobx";
import { IDataViewBodyUI, IDataViewToolbarUI } from "modules/DataView/DataViewUI";
import { TypeSymbol } from "dic/Container";
import { SectionViewSwitchers } from "modules/DataView/DataViewTypes";
import { getIdent, IIId } from "utils/common";
import { DataViewHeaderAction } from "gui/Components/DataViewHeader/DataViewHeaderAction";
import { Icon } from "gui/Components/Icon/Icon";
import { Observer } from "mobx-react";
import { ITablePerspective } from "./TablePerspective";
import { IPerspective } from "../Perspective";
import { TableView } from "gui/Workbench/ScreenArea/TableView/TableView";
import { T } from "../../../../utils/translation";
import S from "./TablePerspectiveDirector.module.scss";
import cx from "classnames";

export class TablePerspectiveDirector implements IIId {
  $iid = getIdent();

  constructor(
    public dataViewToolbarUI = IDataViewToolbarUI(),
    public dataViewBodyUI = IDataViewBodyUI(),
    public tablePerspective = ITablePerspective(),
    public perspective = IPerspective()
  ) {
  }

  @action.bound
  setup() {
    this.dataViewBodyUI.contrib.put({
      $iid: this.$iid,
      render: () => (
        <Observer key={this.$iid}>
          {() => (
            <div className={cx(S.root, "tablePerspectiveDirector", {isActive: this.tablePerspective.isActive})}>
              <TableView/>
            </div>
          )}
        </Observer>
      ),
    });

    this.dataViewToolbarUI.contrib.put({
      $iid: this.$iid,
      section: SectionViewSwitchers,
      render: () => (
        <Observer key={this.$iid}>
          {() => (
            <DataViewHeaderAction
              className={"tablePerspectiveButton"}
              onMouseDown={flow(this.tablePerspective.handleToolbarBtnClick)}
              isActive={this.tablePerspective.isActive}
            >
              <Icon src="./icons/table-view.svg" tooltip={T("Grid", "grid_tool_tip")}/>
            </DataViewHeaderAction>
          )}
        </Observer>
      ),
    });

    this.perspective.contrib.put(this.tablePerspective);
  }

  @action.bound
  teardown() {
    this.dataViewBodyUI.contrib.del(this);
    this.dataViewToolbarUI.contrib.del(this);
    this.perspective.contrib.del(this.tablePerspective);
  }

  dispose() {
    this.teardown();
  }
}

export const ITablePerspectiveDirector = TypeSymbol<TablePerspectiveDirector>(
  "ITablePerspectiveDirector"
);
