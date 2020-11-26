import React from "react";
import { ModalWindow } from "../Dialog/Dialog";
import { observer } from "mobx-react";
import CS from "./DialogsCommon.module.css";
import { T } from "../../../utils/translation";

@observer
export class QuestionSaveData extends React.Component<{
  screenTitle: string;
  onSaveClick?: (event: any) => void;
  onDontSaveClick?: (event: any) => void;
  onCancelClick?: (event: any) => void;
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
              autoFocus={true}
              tabIndex={0}
              ref={this.refPrimaryBtn}
              onClick={this.props.onSaveClick}
            >
              {T("Save", "save")}
            </button>
            <button tabIndex={0} onClick={this.props.onDontSaveClick}>
              {T("Don't Save", "close_without_saving")}
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
          {T("Do you wish to save changes in {0}?", "do_you_wish_to_save", this.props.screenTitle)}
        </div>
      </ModalWindow>
    );
  }
}
