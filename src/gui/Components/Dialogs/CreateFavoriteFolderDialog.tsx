import { observer } from "mobx-react";
import React from "react";
import { observable } from "mobx";
import { ModalWindow } from "gui/Components/Dialog/Dialog";
import { T } from "utils/translation";
import CS from "gui/Components/Dialogs/DialogsCommon.module.css";
import S from "gui/Components/Dialogs/SaveFilterDialog.module.css";

@observer
export class CreateFavoriteFolderDialog extends React.Component<{
  onCancelClick: (event: any) => void;
  onOkClick: (name: string) => void;
}> {
  @observable
  groupName: string = "";

  refInput = React.createRef<HTMLInputElement>();

  onNameChanged(event: any) {
    this.groupName = event.target.value;
  }

  componentDidMount() {
    this.refInput.current?.focus();
  }

  onKeydown(event: React.KeyboardEvent<HTMLInputElement>) {
    if (event.key === "Enter") {
      this.props.onOkClick(this.groupName);
    }
  }

  render() {
    return (
      <ModalWindow
        title={T("New Favourites Folder", "new_group_title")}
        titleButtons={null}
        buttonsCenter={
          <>
            <button onClick={() => this.props.onOkClick(this.groupName)}>
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
              <input
                ref={this.refInput}
                className={S.textInput}
                value={this.groupName}
                onChange={(event) => this.onNameChanged(event)}
                onKeyDown={(event: React.KeyboardEvent<HTMLInputElement>) => this.onKeydown(event)}
              />
            </div>
          </div>
        </div>
      </ModalWindow>
    );
  }
}