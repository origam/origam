import React from "react";
import { ModalWindowOverlay, ModalWindow, CloseButton } from "../Dialog/Dialog";
import { observer } from "mobx-react";
import { getDialogStack } from "../../../model/selectors/DialogStack/getDialogStack";
import CS from "./DialogsCommon.module.css";

@observer
export class QuestionDeleteData extends React.Component<{
  screenTitle: string;
  onNoClick?: (event: any) => void;
  onYesClick?: (event: any) => void;
}> {
  render() {
    return (
      <ModalWindow
        title="Question"
        titleButtons={null}
        buttonsCenter={
          <>
            <button onClick={this.props.onYesClick}>Yes</button>
            <button onClick={this.props.onNoClick}>No</button>
          </>
        }
        buttonsLeft={null}
        buttonsRight={null}
      >
        <div className={CS.dialogContent}>Are you sure you want to delete this record?</div>
      </ModalWindow>
    );
  }
}
