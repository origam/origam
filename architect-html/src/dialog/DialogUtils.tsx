/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

import { action } from "mobx";
import { YesNoQuestion } from "src/dialog/components/YesNoQuestion.tsx";
import { IDialogStackState } from "src/dialog/types.ts";

export function askYesNoQuestion(
  dialogStack: IDialogStackState, title: string, question: string): Promise<YesNoResult>
{
  return new Promise(
    action((resolve: (value: YesNoResult) => void) => {
        const closeDialog = dialogStack.pushDialog(
          "",
          <YesNoQuestion
            screenTitle={title}
            yesLabel={"Yes"}
            noLabel={"No"}
            cancelLabel={"Cancel"}
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
          />
        );
      }
    )
  );
}

export enum YesNoResult {
  Yes,
  No,
  Cancel
}