import {observer} from "mobx-react";
import React from "react";
import {observable} from "mobx";
import {ModalWindow} from "gui/Components/Dialog/Dialog";
import {T} from "utils/translation";
import CS from "gui/Components/Dialogs/DialogsCommon.module.css";
import S from "gui/Components/Dialogs/FavoriteFolderPropertiesDialog.module.scss";
import {Tooltip} from "react-tippy";

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
    this.refInput.current?.focus();
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
    this.props.onOkClick(this.groupName)
  }

  render() {
    return (
      <ModalWindow
        title={T("New Column Configuration", "column_config_new_config_name")}
        titleButtons={null}
        buttonsCenter={
          <>
            <button tabIndex={0} onClick={() => this.onOkClick()}>
              {T("Ok", "button_ok")}
            </button>
            <button tabIndex={0}
                    onClick={this.props.onCancelClick}>{T("Cancel", "button_cancel")}</button>
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
                value={this.groupName}
                onChange={(event) => this.onNameChanged(event)}
                onKeyDown={(event: React.KeyboardEvent<HTMLInputElement>) => this.onKeydown(event)}
              />
              {this.isInvalid && (
                <div>
                  <div className={S.notification}>
                    <Tooltip html={T("Name cannot be empty", "column_config_name_empty",)}
                             arrow={true}>
                      <i className="fas fa-exclamation-circle red"/>
                    </Tooltip>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      </ModalWindow>
    );
  }
}