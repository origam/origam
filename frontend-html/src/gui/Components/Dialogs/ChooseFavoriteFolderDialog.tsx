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
import S from "gui/Components/Dialogs/SaveFilterDialog.module.css";
import { FavoriteFolder } from "model/entities/Favorites";
import { ModalDialog } from "gui/Components/Dialog/ModalDialog";
import { IOption, SimpleDropdown } from "gui/Components/Dialogs/SimpleDropdown";

@observer
export class ChooseFavoriteFolderDialog extends React.Component<{
  onCancelClick: (event: any) => void;
  onOkClick: (selectdFolderId: string) => void;
  favorites: FavoriteFolder[];
}> {
  options: IOption<FavoriteFolder>[];

  @observable
  selectedOption: IOption<FavoriteFolder>;

  constructor(props: any) {
    super(props);
    this.options = this.props.favorites.map((favorite) => {
      return {value: favorite, label: favorite.name};
    });
    this.selectedOption = this.options[0];
  }

  onKeydown(event: React.KeyboardEvent<HTMLSelectElement>) {
    if (event.key === "Enter") {
      this.props.onOkClick(this.selectedOption.value.id);
    }
  }

  render() {
    return (
      <ModalDialog
        title={T("Select Favourites Folder", "select_group_title")}
        titleButtons={null}
        buttonsCenter={
          <>
            <button
              tabIndex={0}
              autoFocus={true}
              onClick={() => this.props.onOkClick(this.selectedOption.value.id)}
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
              <div className={S.label}>{T("Name:", "group_name")}</div>
              <SimpleDropdown
                width={"150px"}
                options={this.options}
                selectedOption={this.selectedOption}
                onOptionClick={(option) => (this.selectedOption = option)}
              />
            </div>
          </div>
        </div>
      </ModalDialog>
    );
  }
}
