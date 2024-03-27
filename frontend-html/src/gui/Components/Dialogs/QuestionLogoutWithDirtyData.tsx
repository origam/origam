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
import { observer } from "mobx-react";
import CS from "./DialogsCommon.module.css";
import { T } from "../../../utils/translation";
import { ModalDialog } from "gui/Components/Dialog/ModalDialog";
import { requestFocus } from "utils/focus";

@observer
export class QuestionLogoutWithDirtyData extends React.Component<{
  onNoClick?: (event: any) => void;
  onYesClick?: (event: any) => void;
}> {
  refPrimaryBtn = (elm: any) => (this.elmPrimaryBtn = elm);
  elmPrimaryBtn: any;

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
        title={T("Question", "question_title")}
        titleButtons={null}
        buttonsCenter={
          <>
            <button
              ref={this.refPrimaryBtn}
              tabIndex={0}
              autoFocus={true}
              onClick={this.props.onYesClick}
            >
              {T("Yes", "button_yes")}
            </button>
            <button tabIndex={0} onClick={this.props.onNoClick}>
              {T("No", "button_no")}
            </button>
          </>
        }
        buttonsLeft={null}
        buttonsRight={null}
      >
        <div className={CS.dialogContent}>
          {T(
            "Some changes has not been saved yet. Do you still want to sign out?",
            "sign_out_data_lost_question"
          )}
        </div>
      </ModalDialog>
    );
  }
}
