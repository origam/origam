import { observer, Provider } from "mobx-react";
import React from "react";
import { observable } from "mobx";
import { ModalWindow } from "gui/Components/Dialog/Dialog";
import { T } from "utils/translation";
import CS from "gui/Components/Dialogs/DialogsCommon.module.css";
import S from "gui/Components/Dialogs/SaveFilterDialog.module.css";
import { FavoriteFolder } from "model/entities/Favorites";
import { SimpleDropdown, IOption } from "modules/Editors/SimpleDropdown";

@observer
export class ChooseFavoriteFolderDialog extends React.Component<{
  onCancelClick: (event: any) => void;
  onOkClick: (selectdFolderId: string) => void;
  favorites: FavoriteFolder[];
}> {
  options: IOption<FavoriteFolder>[];

  @observable
  selectedOption: IOption<FavoriteFolder>;

  refInput = React.createRef<HTMLSelectElement>();

  constructor(props: any) {
    super(props);
    this.options = this.props.favorites.map((favorite) => {
      return { value: favorite, label: favorite.name };
    });
    this.selectedOption = this.options[0];
  }

  componentDidMount() {
    this.refInput.current?.focus();
  }

  onKeydown(event: React.KeyboardEvent<HTMLSelectElement>) {
    if (event.key === "Enter") {
      this.props.onOkClick(this.selectedOption.value.id);
    }
  }

  refPrimaryBtn = (elm: any) => (this.elmPrimaryBtn = elm);
  elmPrimaryBtn: any;


  render() {
    return (
      <ModalWindow
        title={T("Select Favourites Folder", "select_group_title")}
        titleButtons={null}
        buttonsCenter={
          <>
            <button
              tabIndex={0}
              autoFocus={true}
              ref={this.refPrimaryBtn}
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
      </ModalWindow>
    );
  }
}
