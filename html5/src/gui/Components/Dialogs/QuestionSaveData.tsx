import React from "react";
import { ModalWindowOverlay, ModalWindow, CloseButton } from "../Dialog/Dialog";
import { observer } from "mobx-react";
import { getDialogStack } from "../../../model/selectors/DialogStack/getDialogStack";
import CS from "./DialogsCommon.module.css";

@observer
export class QuestionSaveData extends React.Component<{
  screenTitle: string;
  onSaveClick?: (event: any) => void;
  onDontSaveClick?: (event: any) => void;
  onCancelClick?: (event: any) => void;
}> {
  render() {
    return (
      <ModalWindow
        title="Question"
        titleButtons={null}
        buttonsCenter={
          <>
            <button onClick={this.props.onSaveClick}>Save</button>
            <button onClick={this.props.onDontSaveClick}>Don't Save</button>
            <button onClick={this.props.onCancelClick}>Cancel</button>
          </>
        }
        buttonsLeft={null}
        buttonsRight={null}
      >
        <div className={CS.dialogContent}>
          Do you wish to save changes in {this.props.screenTitle}
        </div>
      </ModalWindow>
    );
  }
}
