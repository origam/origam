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
import { observable } from "mobx";
import { T } from "utils/translation";
import CS from "gui/Components/Dialogs/DialogsCommon.module.css";
import S from "gui/Components/Dialogs/FavoriteFolderPropertiesDialog.module.scss";
import { ModalDialog } from "gui/Components/Dialog/ModalDialog";
import { requestFocus } from "utils/focus";

@observer
export class FavoriteFolderPropertiesDialog extends React.Component<{
  title: string;
  name?: string;
  isPinned?: boolean;
  onCancelClick: (event: any) => void;
  onOkClick: (name: string, isPinned: boolean) => void;
}> {
  @observable
  groupName: string = this.props.name ?? "";

  @observable
  isPinned: boolean = this.props.isPinned ?? false;

  get isInvalid() {
    return this.groupName === "";
  }

  refInput = React.createRef<HTMLInputElement>();

  onNameChanged(event: any) {
    this.groupName = event.target.value;
  }

  onIsPinnedClicked(event: any) {
    this.isPinned = event.target.checked;
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
    this.props.onOkClick(this.groupName, this.isPinned);
  }

  render() {
    return (
      <ModalDialog
        title={this.props.title}
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
              <div className={S.label}>{T("Name:", "group_name")}</div>
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
                    title={T("Name cannot be empty", "group_name_empty")}
                  >
                    <i className="fas fa-exclamation-circle red"/>
                  </div>
                </div>
              )}
            </div>
            <div id={S.lastRow} className={S.row}>
              <div className={S.label}>{T("Pin to the Top:", "group_pinned")}</div>
              <input
                className={S.chcekBoxinput}
                type="checkbox"
                checked={this.isPinned}
                onChange={(event) => this.onIsPinnedClicked(event)}
              />
            </div>
          </div>
        </div>
      </ModalDialog>
    );
  }
}
