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
import { T } from "utils/translation";
import CS from "gui/Components/Dialogs/DialogsCommon.module.css";
import S from "gui/Components/Dialogs/SaveFilterDialog.module.css";
import { observable } from "mobx";
import { ModalDialog } from "gui/Components/Dialog/ModalDialog";
import { requestFocus } from "utils/focus";

@observer
export class SaveFilterDialog extends React.Component<{
  onCancelClick: (event: any) => void;
  onOkClick: (name: string, isGlobal: boolean) => void;
}> {
  @observable
  filterName: string = "";

  @observable
  isGlobal: boolean = false;

  refInput = React.createRef<HTMLInputElement>();

  onNameChanged(event: any) {
    this.filterName = event.target.value;
  }

  onIsGlobalClicked(event: any) {
    this.isGlobal = event.target.checked;
  }

  componentDidMount() {
    requestFocus(this.refInput.current);
  }

  onKeydown(event: React.KeyboardEvent<HTMLInputElement>) {
    if (event.key === "Enter") {
      this.props.onOkClick(this.filterName, this.isGlobal);
    }
  }

  render() {
    return (
      <ModalDialog
        title={T("New Filter", "new_filter_title")}
        titleButtons={null}
        buttonsCenter={
          <>
            <button
              tabIndex={0}
              autoFocus={true}
              onClick={() => this.props.onOkClick(this.filterName, this.isGlobal)}
            >
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
              <div className={S.label}>{T("Name:", "new_filter_name")}</div>
              <input
                ref={this.refInput}
                className={S.textInput}
                autoComplete={"off"}
                value={this.filterName}
                onChange={(event) => this.onNameChanged(event)}
                onKeyDown={(event: React.KeyboardEvent<HTMLInputElement>) => this.onKeydown(event)}
              />
            </div>
            <div className={S.row}>
              <div className={S.label}>{T("Global:", "new_filter_global")}</div>
              <input
                className={S.chcekBoxinput}
                type="checkbox"
                checked={this.isGlobal}
                onChange={(event) => this.onIsGlobalClicked(event)}
              />
            </div>
          </div>
        </div>
      </ModalDialog>
    );
  }
}
