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

import { T } from '@/main';
import { YesNoQuestion } from '@dialogs/components/YesNoQuestion';
import { IDialogStackState } from '@dialogs/types';
import { action } from 'mobx';
import { Info } from '@/dialog/components/Info.tsx';

export function askYesNoQuestion(
  dialogStack: IDialogStackState,
  title: string,
  question: string,
): Promise<YesNoResult> {
  return new Promise(
    action((resolve: (value: YesNoResult) => void) => {
      const closeDialog = dialogStack.pushDialog(
        '',
        <YesNoQuestion
          screenTitle={title}
          yesLabel={T('Yes', 'dialog_yes')}
          noLabel={T('No', 'dialog_no')}
          cancelLabel={T('Cancel', 'dialog_cancel')}
          message={question}
          onYesClick={() => {
            closeDialog();
            resolve(YesNoResult.Yes);
          }}
          onNoClick={() => {
            closeDialog();
            resolve(YesNoResult.No);
          }}
          onCancelClick={() => {
            closeDialog();
            resolve(YesNoResult.Cancel);
          }}
        />,
      );
    }),
  );
}

export enum YesNoResult {
  Yes,
  No,
  Cancel,
}

export function showInfo(
  dialogStack: IDialogStackState,
  title: string,
  text: string,
): Promise<YesNoResult> {
  return new Promise(
    action(() => {
      const closeDialog = dialogStack.pushDialog(
        '',
        <Info
          screenTitle={title}
          okLabel={T('Ok', 'dialog_ok')}
          message={text}
          onCancelClick={() => {
            closeDialog();
          }}
        />,
      );
    }),
  );
}
