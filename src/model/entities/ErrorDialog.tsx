import bind from "bind-decorator";
import { ModalWindow } from "gui/Components/Dialog/Dialog";
import _ from "lodash";
import { action, observable, computed } from "mobx";
import { observer, Observer } from "mobx-react";
import { getDialogStack } from "model/selectors/getDialogStack";
import React from "react";
import CS from "./ErrorDialog.module.scss";
import moment, { Moment } from "moment";

function NewExternalPromise<T>() {
  let resolveFn: any;
  let rejectFn: any;
  const p = new Promise((resolve, reject) => {
    resolveFn = resolve;
    rejectFn = reject;
  });
  (p as any).resolve = resolveFn;
  (p as any).reject = rejectFn;
  return p as Promise<T> & {
    resolve(value: T): void;
    reject(error: any): void;
  };
}

export class ErrorDialogController {
  @observable errorStack: Array<{
    id: number;
    error: any;
    promise: any;
    timestamp: Moment;
  }> = [];

  @observable isDialogDisplayed = false;

  @computed get errorMessages() {
    return this.errorStack.map(errItem => {
      const responseMessage =
        _.get(errItem.error, "response.data.message") ||
        _.get(errItem.error, "response.data.Message");

      const errorMessage = responseMessage || "" + errItem.error;
      return {
        message: errorMessage,
        id: errItem.id,
        timestamp: errItem.timestamp.format("YYYY-MM-DD HH:mm:ss")
      };
    });
  }

  idGen = 0;
  @bind
  *pushError(error: any) {
    const myId = this.idGen++;
    const promise = NewExternalPromise();
    this.errorStack.push({ id: myId, error, promise, timestamp: moment() });
    this.displayDialog();
    console.log(this.errorMessages.length);
    yield promise;
  }

  @action.bound displayDialog() {
    if (!this.isDialogDisplayed) {
      this.isDialogDisplayed = true;
      const closeDialog = getDialogStack(this).pushDialog(
        "",
        <Observer>
          {() => (
            <ErrorDialogComponent
              errorMessages={this.errorMessages}
              onOkClick={action(() => {
                console.log("close dialog...");
                closeDialog();
                this.isDialogDisplayed = false;
                this.dismissErrors();
              })}
            />
          )}
        </Observer>
      );
    }
  }

  @action.bound
  dismissError(id: number) {
    const errItemIndex = this.errorStack.findIndex(item => item.id === id);
    if (errItemIndex > -1) {
      const errItem = this.errorStack[errItemIndex];
      errItem.promise.resolve(null);
      this.errorStack.splice(errItemIndex, 1);
    }
  }

  @action.bound dismissErrors() {
    for (let errItem of [...this.errorStack]) {
      this.dismissError(errItem.id);
    }
  }
}

/*
export const errDialogPromise = (ctx: any) => (error: any) =>
  new Promise(resolve => {
    const responseMessage =
      _.get(error, "response.data.message") ||
      _.get(error, "response.data.Message");

    const errorMessage = responseMessage || "" + error;
    const closeDialog = getDialogStack(ctx).pushDialog(
      "",
      <ErrorDialogComponent
        errorMessages={errorMessages}
        onOkClick={action(() => {
          console.log("close dialog...");
          closeDialog();
          resolve();
        })}
      />
    );
  });*/

@observer
export class ErrorDialogComponent extends React.Component<{
  errorMessages: Array<{ id: number; message: string; timestamp: string }>;
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
        <div className={CS.dialogContent}>
          {this.props.errorMessages.length === 1 ? (
            this.props.errorMessages[0].message
          ) : (
            <div className={CS.errorMessageList}>
              {this.props.errorMessages.map(errMessage => (
                <div key={errMessage.id} className={CS.errorMessageListItem}>
                  <span className={CS.errorMessageDatetime}>
                    {errMessage.timestamp}
                  </span>
                  {errMessage.message}
                </div>
              ))}
            </div>
          )}
        </div>
      </ModalWindow>
    );
  }
}
