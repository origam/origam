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

@observer
export class SelectionCheckBoxHeader extends React.Component<{
  width: number;
  dataView: any;
}> {
  @action.bound
  onClick(event: any) {
    this.props.dataView.selectAllCheckboxChecked = !this.props.dataView.selectAllCheckboxChecked;
    setAllSelectionStates(this.props.dataView, this.props.dataView.selectAllCheckboxChecked);
  }

  render() {
    const isChecked = this.props.dataView.selectAllCheckboxChecked;
    return (
      <div style={{minWidth: this.props.width + "px"}} className={S.root} onClick={this.onClick}>
        <div className={S.allChecker}>
          {isChecked ? <i className="far fa-check-square"/> : <i className="far fa-square"/>}
        </div>
        <div className={S.filter}>
          <i className="far fa-minus-square" />
        </div>
        
      </div>
    );
  }
}
