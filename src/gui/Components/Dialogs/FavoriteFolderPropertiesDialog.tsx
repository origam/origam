import { observer } from "mobx-react";
import React from "react";
import { observable } from "mobx";
import { ModalWindow } from "gui/Components/Dialog/Dialog";
import { T } from "utils/translation";
import CS from "gui/Components/Dialogs/DialogsCommon.module.css";
import S from "gui/Components/Dialogs/FavoriteFolderPropertiesDialog.module.scss";
import {Tooltip} from "react-tippy";

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

  get isInvalid(){
    return this.groupName === "";
  }

  refInput = React.createRef<HTMLInputElement>();

  onNameChanged(event: any) {
    this.groupName = event.target.value;
  }

  onIsPinnedClicked(event: any){
    this.isPinned = event.target.checked;
  }

  componentDidMount() {
    this.refInput.current?.focus();
  }

  onKeydown(event: React.KeyboardEvent<HTMLInputElement>) {
    if (event.key === "Enter") {
      this.onOkClick();
    }
  }

  onOkClick(){
    if(this.isInvalid){
      return;
    }
    this.props.onOkClick(this.groupName, this.isPinned)
  }

  render() {
    return (
      <ModalWindow
        title={this.props.title}
        titleButtons={null}
        buttonsCenter={
          <>
            <button tabIndex={0} autoFocus={true} onClick={() => this.onOkClick()}>
              {T("Ok", "button_ok")}
            </button>
            <button tabIndex={0} onClick={this.props.onCancelClick}>{T("Cancel", "button_cancel")}</button>
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
              {this.isInvalid && (
                <div>
                <div className={S.notification}>
                  <Tooltip html={ T("Name cannot be empty", "group_name_empty")} arrow={true}>
                    <i className="fas fa-exclamation-circle red" />
                  </Tooltip>
                </div>
                </div>
              )}
            </div>
            <div id={S.lastRow} className={S.row}>
              <div className={S.label}>
                {T("Pin to the Top:", "group_pinned")}
              </div>
              <input
                className={S.chcekBoxinput}
                type="checkbox"
                checked={this.isPinned}
                onChange={event => this.onIsPinnedClicked(event)}
              />
            </div>
          </div>
        </div>
      </ModalWindow>
    );
  }
}