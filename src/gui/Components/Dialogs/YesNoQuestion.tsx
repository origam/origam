import { observer } from "mobx-react";
import React from "react";
import { ModalWindow } from "gui/Components/Dialog/Dialog";
import { T } from "utils/translation";
import CS from "gui/Components/Dialogs/DialogsCommon.module.css";

@observer
export class YesNoQuestion extends React.Component<{
  screenTitle: string;
  message: string;
  onYesClick?: (event: any) => void;
  onNoClick?: (event: any) => void;
}> {
  render() {
    return (
      <ModalWindow
        title={T("Question", "question_title")}
        titleButtons={null}
        buttonsCenter={
          <>
            <button onClick={this.props.onYesClick}>{T("Yes", "button_yes")}</button>
            <button onClick={this.props.onNoClick}>{T("No", "button_no")}</button>
          </>
        }
        buttonsLeft={null}
        buttonsRight={null}
      >
        <div className={CS.dialogContent}>
          {this.props.message}
        </div>
      </ModalWindow>
    );
  }
}