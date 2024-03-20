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
import React from "react";
import CS from "gui/Components/Dialogs/DialogsCommon.module.css";
import { ModalDialog } from "gui/Components/Dialog/ModalDialog";
import { requestFocus } from "utils/focus";

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
        requestFocus(this.elmPrimaryBtn);
      }
    }, 150);
  }

  render() {
    return (
      <ModalDialog
        title={this.props.screenTitle}
        titleButtons={null}
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
      </ModalDialog>
    );
  }
}
