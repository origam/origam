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

import { WindowsSvc } from "../components/Windows/WindowsSvc";
import { ChatHTTPApi } from "../services/ChatHTTPApi";
import {
  renderMentionUserDialog,
  UserToMention,
} from "../components/Dialogs/MentionUserDialog";
import { renderErrorDialog } from "../components/Dialogs/ErrorDialog";

export class MentionUserWorkflow {
  constructor(public windowsSvc: WindowsSvc, public api: ChatHTTPApi) {}

  async start(feedUsersToMention: (users: UserToMention[]) => void) {
    const mentionDialog = this.windowsSvc.push(renderMentionUserDialog());
    try {
      while (true) {
        try {
          const mentionDialogResult = await mentionDialog.interact();
          if (mentionDialogResult.choosenUsers) {
            feedUsersToMention(mentionDialogResult.choosenUsers);
            return;
          }
          if (mentionDialogResult.isCancel) return;
        } catch (e) {
          console.error(e);
          const errDlg = this.windowsSvc.push(renderErrorDialog(e));
          await errDlg.interact();
          errDlg.close();
        }
      }
    } finally {
      mentionDialog.close();
    }
  }
}
