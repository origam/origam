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
    const responseMessage =
      _.get(event, "error.response.data.message") ||
      _.get(event, "error.response.data.Message");
    const exceptionMessage = _.get(event, "message");
    const errorMessage =
      responseMessage || exceptionMessage || "" + event.error;

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

export const errDialogPromise = (ctx: any) => (error: any) =>
  new Promise(resolve => {
    const responseMessage =
      _.get(error, "response.data.message") ||
      _.get(error, "response.data.Message");

    const errorMessage = responseMessage || "" + error;
    const closeDialog = getDialogStack(ctx).pushDialog(
      "",
      <ErrorDialogComponent
        errorMessage={errorMessage}
        onOkClick={action(() => {
          console.log('close dialog...')
          closeDialog();
        })}
      />
    );
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
