import _ from "lodash";
import { getDialogStack } from "model/selectors/getDialogStack";
import { observer } from "mobx-react";
import React from "react";
import { ModalWindow } from "gui/Components/Dialog/Dialog";
import CS from "./ErrorDialog.module.css";
import { action } from "mobx";

export function ErrorStateDef() {
  return {
    initial: "errDialog",
    states: {
      errDialog: {
        invoke: { src: "errDialog" },
        on: {
          onErrDialogOK: "ok"
        }
      },
      ok: {
        type: "final"
      }
    }
  };
}

export const errDialogSvc = (ctx: any) => ({
  errDialog: action((mCtx: any, event: any) => (send: any, onEvent: any) => {
    const responseData = _.get(event, "error.response.data");
    const errorMessage = responseData ? responseData.message : "Error";
    if (responseData) {
      console.log("Show dialog:", responseData.message);
    } else {
      console.log("Show dialog - general:", event.error);
    }
    return getDialogStack(ctx).pushDialog(
      "error_dialog",
      <ErrorDialogComponent
        errorMessage={errorMessage}
        onOkClick={action(() => {
          send("onErrDialogOK");
        })}
      />
    );
  })
});

@observer
export class ErrorDialogComponent extends React.Component<{
  errorMessage: string;
  onOkClick?: (event: any) => void;
}> {
  render() {
    return (
      <ModalWindow
        title="Error"
        titleButtons={null}
        buttonsCenter={
          <>
            <button onClick={this.props.onOkClick}>OK</button>
          </>
        }
        buttonsLeft={null}
        buttonsRight={null}
      >
        <div className={CS.dialogContent}>{this.props.errorMessage}</div>
      </ModalWindow>
    );
  }
}
