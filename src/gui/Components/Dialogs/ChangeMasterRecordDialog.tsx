import { observer } from "mobx-react";
import React from "react";
import { ModalWindow } from "../Dialog/Dialog";
import CS from "./DialogsCommon.module.css";

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
        title="Question"
        titleButtons={null}
        buttonsCenter={
          <>
            <button onClick={this.props.onSaveClick}>Yes</button>
            <button onClick={this.props.onDontSaveClick}>No</button>
            <button onClick={this.props.onCancelClick}>Cancel</button>
          </>
        }
        buttonsLeft={null}
        buttonsRight={null}
      >
        <div className={CS.dialogContent}>Record has been changed. Do you want to save the changes?
          <br/>
          <br/>
          By clicking Yes the changes will be stored.<br/>
          By clicking No the original record will be kept unchanged.<br/>
          By clicking Cancel you can continue in doing changes.</div>
      </ModalWindow>
    );
  }
}
