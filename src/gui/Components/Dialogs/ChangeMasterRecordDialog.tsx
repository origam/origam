import { observer } from "mobx-react";
import React from "react";
import { ModalWindow } from "../Dialog/Dialog";
import CS from "./DialogsCommon.module.css";
import { T } from "../../../utils/translation";

@observer
export class ChangeMasterRecordDialog extends React.Component<{
  screenTitle: string;
  onSaveClick?: (event: any) => void;
  onDontSaveClick?: (event: any) => void;
  onCancelClick?: (event: any) => void;
}> {
  render() {
    return (
      <ModalWindow
        title={T("Question", "question_title")}
        titleButtons={null}
        buttonsCenter={
          <>
            <button tabIndex={0} autoFocus={true} onClick={this.props.onSaveClick}>
              {T("Yes", "button_yes")}
            </button>
            <button tabIndex={0} onClick={this.props.onDontSaveClick}>
              {T("No", "button_no")}
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
          {" "}
          {T("Record has been changed. Do you want to save the changes?", "record_changed_info")
            .split("\\n")
            .join("\n")}
        </div>
      </ModalWindow>
    );
  }
}
