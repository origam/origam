/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import _ from 'lodash';
import { action, computed, observable } from 'mobx';
import { observer, Observer } from 'mobx-react-lite';
import React from 'react';
import S from './ErrorDialog.module.scss';
import { requestFocus } from 'src/utils/focus.ts';
import { IDialogStackState } from 'src/dialog/types.ts';
import { Icon } from 'src/components/icon/Icon.tsx';
import { ModalWindow } from 'src/dialog/ModalWindow.tsx';

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
  constructor(private dialogStack: IDialogStackState) {}

  @observable accessor errorStack: Array<{
    id: number;
    error: any;
    promise: any;
    timestamp: Date;
  }> = [];

  @observable accessor isDialogDisplayed = false;

  @computed get errorMessages() {
    return this.errorStack.map(errItem => {
      console.error(errItem.error);

      const handlePlainText = () =>
        _.get(errItem.error, 'response.headers.content-type', '').startsWith('text/plain') &&
        errItem.error.response.data;

      const handleMessageField = () => {
        if (!errItem.error.response?.data) {
          return '';
        }
        let exception = errItem.error.response.data;
        let message = '';
        do {
          const exMessage = _.get(exception, 'message') || _.get(exception, 'Message');
          if (exMessage) {
            message += exMessage;
            message += '\n';
          }
          exception = exception.innerException || exception.InnerException;
        } while (exception);
        return message;
      };

      const handleRuntimeException = () => '' + errItem.error;

      let errorMessage = handlePlainText() || handleMessageField() || handleRuntimeException();

      if (errItem.error?.request?.status === 500 || errItem.error?.request?.status === 409) {
        errorMessage = 'Server error occurred. Please check server log for more details.';
      }

      return {
        message: errorMessage,
        id: errItem.id,
        timestamp: errItem.timestamp.toLocaleString(),
      };
    });
  }

  idGen = 0;

  *pushError(error: any) {
    const myId = this.idGen++;
    const promise = NewExternalPromise();

    if (!this.theSameErrorAlreadyDisplayed(error)) {
      this.errorStack.push({ id: myId, error, promise, timestamp: new Date() });
    }
    this.displayDialog();
    yield promise;
  }

  theSameErrorAlreadyDisplayed(error: any) {
    if (this.errorStack.length == 0) {
      return false;
    }

    const lastError = this.errorStack[this.errorStack.length - 1];
    return lastError.error?.name === error.name && lastError.error?.message === error.message;
  }

  @action.bound displayDialog() {
    if (!this.isDialogDisplayed) {
      this.isDialogDisplayed = true;
      const previouslyFocusedElement = document.activeElement as HTMLElement;
      const closeDialog = this.dialogStack.pushDialog(
        '',
        <Observer>
          {() => (
            <ErrorDialogComponent
              errorMessages={this.errorMessages}
              onOkClick={action(() => {
                closeDialog();
                this.isDialogDisplayed = false;
                this.dismissErrors();
                setTimeout(() => requestFocus(previouslyFocusedElement), 100);
              })}
            />
          )}
        </Observer>,
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
    for (const errItem of [...this.errorStack]) {
      this.dismissError(errItem.id);
    }
  }
}

interface ErrorMessageType {
  id: number;
  message: string;
  timestamp: string;
}

interface ErrorDialogComponentProps {
  errorMessages: ErrorMessageType[];
  onOkClick?: (event: any) => void;
}

export const ErrorDialogComponent: React.FC<ErrorDialogComponentProps> = observer(
  ({ errorMessages, onOkClick }) => {
    return (
      <ModalWindow
        title="Error"
        buttonsCenter={
          <>
            <button tabIndex={0} autoFocus={true} onClick={onOkClick}>
              Ok
            </button>
          </>
        }
      >
        <div className={S.dialogContent}>
          <div className={S.dialogBigIconColumn}>
            {/* SVG as data url here because we might not be able to determine customAssets path.
          This has fill color embedded */}
            <Icon
              src={`data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4KPCEtLSBHZW5lcmF0b3I6IEFkb2JlIElsbHVzdHJhdG9yIDIzLjEuMSwgU1ZHIEV4cG9ydCBQbHVnLUluIC4gU1ZHIFZlcnNpb246IDYuMDAgQnVpbGQgMCkgIC0tPgo8c3ZnIHZlcnNpb249IjEuMSIgaWQ9IkViZW5lXzEiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyIgeD0iMHB4IiB5PSIwcHgiCiAgICAgdmlld0JveD0iMCAwIDIwIDIwIiBzdHlsZT0iZW5hYmxlLWJhY2tncm91bmQ6bmV3IDAgMCAyMCAyMDsiIHhtbDpzcGFjZT0icHJlc2VydmUiPgo8dGl0bGU+WmVpY2hlbmZsw6RjaGUgMTwvdGl0bGU+CjxnPgoJPHBhdGggZmlsbD0iI2ZmNDM1NCIgZD0iTTkuMywwLjlsLTkuMiwxN2MtMC4zLDAuNSwwLjEsMS4yLDAuNywxLjJoMTguNGMwLjYsMCwxLTAuNywwLjctMS4ybC05LjItMTdDMTAuNCwwLjQsOS42LDAuNCw5LjMsMC45eiBNMTAsMTUuOUwxMCwxNS45CgkJYy0wLjQsMC0wLjgtMC40LTAuOC0wLjh2MGMwLTAuNCwwLjQtMC44LDAuOC0wLjhoMGMwLjQsMCwwLjgsMC40LDAuOCwwLjh2MEMxMC44LDE1LjUsMTAuNCwxNS45LDEwLDE1Ljl6IE05LjIsMTEuOFY2LjkKCQljMC0wLjQsMC40LTAuOCwwLjgtMC44aDBjMC40LDAsMC44LDAuNCwwLjgsMC44djQuOWMwLDAuNC0wLjQsMC44LTAuOCwwLjhoMEM5LjYsMTIuNiw5LjIsMTIuMiw5LjIsMTEuOHoiLz4KPC9nPgo8L3N2Zz4K`}
            />
          </div>
          <div className={`${S.dialogMessageColumn} dialogMessage`}>
            {errorMessages.length === 1 ? (
              errorMessages[0].message
            ) : (
              <div className={S.errorMessageList}>
                {errorMessages.map(errMessage => (
                  <div key={errMessage.id} className={S.errorMessageListItem}>
                    <span className={S.errorMessageDatetime}>{errMessage.timestamp}</span>
                    {errMessage.message}
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </ModalWindow>
    );
  },
);
