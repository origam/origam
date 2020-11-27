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
              tabIndex={0}
              autoFocus={true}
              ref={this.refPrimaryBtn}
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
        <div className={CS.dialogContent}>{this.props.message}</div>
      </ModalWindow>
    );
  }
}
