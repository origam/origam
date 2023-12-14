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
import { showDialog } from "model/selectors/getDialogStack";
import { T } from "utils/translation";
import { YesNoQuestion } from "gui/Components/Dialogs/YesNoQuestion";

export function askYesNoQuestion(ctx: any, title: string, question: string) {
  return new Promise(
    action((resolve: (value: boolean) => void) => {
        const closeDialog = showDialog(ctx,
          "",
          <YesNoQuestion
            screenTitle={title}
            yesLabel={T("Yes", "button_yes")}
            noLabel={T("No", "button_no")}
            message={question}
            onYesClick={() => {
              closeDialog();
              resolve(true);
            }}
            onNoClick={() => {
              closeDialog();
              resolve(false);
            }}
          />
        );
      }
    )
  );
}