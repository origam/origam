import React from "react";
import { ModalWindow } from "../Dialog/Dialog";
import { observer } from "mobx-react";
import CS from "./DialogsCommon.module.css";
import { T } from "../../../utils/translation";

@observer
export class QuestionLogoutWithDirtyData extends React.Component<{
  onNoClick?: (event: any) => void;
  onYesClick?: (event: any) => void;
}> {
  refPrimaryBtn = (elm: any) => (this.elmPrimaryBtn = elm);
  elmPrimaryBtn: any;

  componentDidMount() {
    setTimeout(() => {
      if (this.elmPrimaryBtn) {
        this.elmPrimaryBtn.focus?.();
      }
    }, 150);
  }

  render() {
    return (
      <ModalWindow
        title={T("Question", "question_title")}
        titleButtons={null}
        buttonsCenter={
          <>
            <button
              ref={this.refPrimaryBtn}
              tabIndex={0}
              autoFocus={true}
              onClick={this.props.onYesClick}
            >
              {T("Yes", "button_yes")}
            </button>
            <button tabIndex={0} onClick={this.props.onNoClick}>
              {T("No", "button_no")}
            </button>
          </>
        }
        buttonsLeft={null}
        buttonsRight={null}
      >
        <div className={CS.dialogContent}>
          {T(
            "Some changes has not been saved yet. Do you still want to sign out?",
            "sign_out_data_lost_question"
          )}
        </div>
      </ModalWindow>
    );
  }
}
