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

import CS from "gui/Components/Dialogs/DialogsCommon.module.css";
import S from "gui/Components/Dialogs/FavoriteFolderPropertiesDialog.module.scss";
import { observable } from "mobx";
import { observer } from "mobx-react";
import React from "react";
import { T } from "utils/translation";
import { ModalDialog } from "gui/Components/Dialog/ModalDialog";
import { requestFocus } from "utils/focus";

@observer
export class NewConfigurationDialog extends React.Component<{
  onCancelClick: (event: any) => void;
  onOkClick: (name: string) => void;
}> {
  @observable
  groupName: string = "";

  get isInvalid() {
    return this.groupName === "";
  }

  refInput = React.createRef<HTMLInputElement>();

  onNameChanged(event: any) {
    this.groupName = event.target.value;
  }

  componentDidMount() {
    requestFocus(this.refInput.current);
  }

  onKeydown(event: React.KeyboardEvent<HTMLInputElement>) {
    if (event.key === "Enter") {
      this.onOkClick();
    }
  }

  onOkClick() {
    if (this.isInvalid) {
      return;
    }
    this.props.onOkClick(this.groupName);
  }

  render() {
    return (
      <ModalDialog
        title={T("New Column Configuration", "column_config_new_config_name")}
        titleButtons={null}
        buttonsCenter={
          <>
            <button tabIndex={0} onClick={() => this.onOkClick()}>
              {T("Ok", "button_ok")}
            </button>
            <button tabIndex={0} onClick={this.props.onCancelClick}>
              {T("Cancel", "button_cancel")}
            </button>
          </>
        }
        buttonsLeft={null}
        buttonsRight={null}
      >
        <div className={CS.dialogContent}>
          <div className={S.inpuContainer}>
            <div className={S.row}>
              <div className={S.label}>{T("Name:", "column_config_config_name")}</div>
              <input
                ref={this.refInput}
                className={S.textInput}
                autoComplete={"off"}
                value={this.groupName}
                onChange={(event) => this.onNameChanged(event)}
                onKeyDown={(event: React.KeyboardEvent<HTMLInputElement>) => this.onKeydown(event)}
              />
              {this.isInvalid && (
                <div>
                  <div
                    className={S.notification}
                    title={T("Name cannot be empty", "column_config_name_empty")}
                  >
                    <i className="fas fa-exclamation-circle red"/>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      </ModalDialog>
    );
  }
}
