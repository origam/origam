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

import { observer } from "mobx-react";
import React from "react";
import { setAllSelectionStates } from "model/actions-tree/setAllSelectionStates";
import S from "./SelectionCheckboxHeader.module.scss";
import { action } from "mobx";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { isLazyLoading } from "model/selectors/isLazyLoading";

@observer
export class SelectionCheckBoxHeader extends React.Component<{
  width: number;
  dataView: any;
}> {
  @action.bound
  handleCheckAllClick(event: any) {
    this.props.dataView.selectAllCheckboxChecked = 
      !this.props.dataView.selectAllCheckboxChecked;
    setAllSelectionStates(
      this.props.dataView, 
      this.props.dataView.selectAllCheckboxChecked
    );
  }

  @action.bound 
  handleSelectionCheckboxFilterClick(event: any) {
    const tablePanelView = getTablePanelView(this.props.dataView);
    tablePanelView.filterConfiguration.toggleSelectionCheckboxFilter();
  }

  render() {
    const tablePanelView = getTablePanelView(this.props.dataView);
    const filterControlsDisplayed =
      tablePanelView.filterConfiguration.isFilterControlsDisplayed;

    const {selectionCheckboxFilter} = tablePanelView.filterConfiguration;

    const selectionMember: string | null | undefined =
      this.props.dataView.selectionMember;

    const isChecked = this.props.dataView.selectAllCheckboxChecked;
    return (
      <div style={{minWidth: this.props.width + "px"}} className={S.root}>
        <div className={S.allChecker} onClick={this.handleCheckAllClick}>
          {isChecked ? (
            <i className="far fa-check-square"/>
          ) : (
            <i className="far fa-square"/>
          )}
        </div>
        {!!selectionMember && 
        !isLazyLoading(tablePanelView) && 
        filterControlsDisplayed ? (
          <div className={S.filter} onClick={this.handleSelectionCheckboxFilterClick}>
            {selectionCheckboxFilter === null 
              ? <i className="far fa-minus-square"/>
            : selectionCheckboxFilter === true
              ? <i className="far fa-check-square"/>
            : <i className="far fa-square"/>}
          </div>
        ) : null}
      </div>
    );
  }
}
