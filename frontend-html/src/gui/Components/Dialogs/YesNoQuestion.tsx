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

import { MobXProviderContext, observer } from "mobx-react";
import React, { useContext } from "react";
import { ModalWindow } from "./Dialog";
import CS from "gui/Components/Dialogs/DialogsCommon.module.css";
import { isMobileLayoutActive } from "model/selectors/isMobileLayoutActive";

@observer
export class YesNoQuestion extends React.Component<{
  screenTitle: string;
  yesLabel: string;
  noLabel: string;
  message: string;
  onYesClick?: (event: any) => void;
  onNoClick?: (event: any) => void;
}> {
  refPrimaryBtn = (elm: any) => (this.elmPrimaryBtn = elm);
  elmPrimaryBtn: any;
  static contextType = MobXProviderContext;

  componentDidMount() {
    setTimeout(() => {
      if (this.elmPrimaryBtn) {
        this.elmPrimaryBtn.focus?.();
      }
    }, 150);
  }

  render() {
    return (
      <ModalWindow
        title={this.props.screenTitle}
        titleButtons={null}
        fullScreen={isMobileLayoutActive(this.context.application)}
        buttonsCenter={
          <>
            <button
              id={"yesButton"}
              tabIndex={0}
              autoFocus={true}
              ref={this.refPrimaryBtn}
              onClick={this.props.onYesClick}
            >
              {this.props.yesLabel}
            </button>
            <button
              id={"noButton"}
              tabIndex={0}
              onClick={this.props.onNoClick}
            >
              {this.props.noLabel}
            </button>
          </>
        }
        buttonsLeft={null}
        buttonsRight={null}
      >
        <div className={CS.dialogContent}>{this.props.message}</div>
      </ModalWindow>
    );
  }
}