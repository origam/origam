import { observer } from "mobx-react";
import React from "react";
import { observable } from "mobx";
import { ModalWindow } from "gui/Components/Dialog/Dialog";
import { T } from "utils/translation";
import CS from "gui/Components/Dialogs/DialogsCommon.module.css";
import S from "gui/Components/Dialogs/SaveFilterDialog.module.css";
import { FavoriteFolder } from "model/entities/Favorites";

@observer
export class ChooseFavoriteFolderDialog extends React.Component<{
  onCancelClick: (event: any) => void;
  onOkClick: (selectdFolderId: string) => void;
  favorites: FavoriteFolder[];
}> {
  @observable
  selectdFolderId: string = "";

  refInput = React.createRef<HTMLSelectElement>();

  onNameChanged(event: any) {
    this.selectdFolderId = event.target.value;
  }

  componentDidMount() {
    this.refInput.current?.focus();
  }

  onKeydown(event: React.KeyboardEvent<HTMLSelectElement>) {
    if (event.key === "Enter") {
      this.props.onOkClick(this.selectdFolderId);
    }
  }

  render() {
    return (
      <ModalWindow
        title={T("Select Favourites Folder", "select_group_title")}
        titleButtons={null}
        buttonsCenter={
          <>
            <button onClick={() => this.props.onOkClick(this.selectdFolderId)}>
              {T("Ok", "button_ok")}
            </button>
            <button onClick={this.props.onCancelClick}>{T("Cancel", "button_cancel")}</button>
          </>
        }
        buttonsLeft={null}
        buttonsRight={null}
      >
        <div className={CS.dialogContent}>
          <div className={S.inpuContainer}>
            <div className={S.row}>
              <div className={S.label}>{T("Name:", "group_name")}</div>

              {/*<div className="Dropdown_control">*/}
              {/*  <input className="input" placeholder="" value=""/>*/}
              {/*  <div className="inputBtn lastOne">*/}
              {/*    <i className="fas fa-caret-down"></i>*/}
              {/*  </div>*/}
              {/*</div>*/}

              <select
                ref={this.refInput}
                onChange={(event) => this.onNameChanged(event)}
                onKeyDown={(event: React.KeyboardEvent<HTMLSelectElement>) => this.onKeydown(event)}
              >
                {this.props.favorites.map(folder => <option value={folder.id}>{folder.name}</option>)}
              </select>
            </div>
          </div>
        </div>
      </ModalWindow>
    );
  }
}