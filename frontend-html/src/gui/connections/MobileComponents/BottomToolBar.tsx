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
import S from "./BottomToolBar.module.scss";
import { BottomIcon } from "gui/connections/MobileComponents/BottomIcon";
import { MobileState } from "model/entities/MobileState";
import { ActionDropUp } from "gui/connections/MobileComponents/ActionDropUp";
import { observer } from "mobx-react";

@observer
export class BottomToolBar extends React.Component<{
  mobileState: MobileState
}> {
  render() {
    return (
      <div className={S.root}>
        <BottomIcon
          iconPath={"./icons/noun-close-25798.svg"}
          onClick={async () => {
            await this.props.mobileState.close()
          }}
        />
        <ActionDropUp
          hidden={this.props.mobileState.layoutState.actionDropUpHidden}
        />
        <BottomIcon
          iconPath={"./icons/noun-loading-1780489.svg"}
          hidden={this.props.mobileState.layoutState.refreshButtonHidden}
          onClick={() => {
          }}
        />
        <BottomIcon
          iconPath={"./icons/noun-save-1014816.svg"}
          hidden={this.props.mobileState.layoutState.saveButtonHidden}
          onClick={() => {
          }}
        />
      </div>
    );
  }
}



