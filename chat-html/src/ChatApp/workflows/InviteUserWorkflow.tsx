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

import { T, TR } from "util/translation";
import { renderErrorDialog } from "../components/Dialogs/ErrorDialog";
import { renderInviteUserDialog } from "../components/Dialogs/InviteUserDialog";
import {
  renderSimpleInformation,
  renderSimpleProgress,
} from "../components/Windows/Windows";
import { WindowsSvc } from "../components/Windows/WindowsSvc";
import { ChatHTTPApi } from "../services/ChatHTTPApi";

export class InviteUserWorkflow {
  constructor(public windowsSvc: WindowsSvc, public api: ChatHTTPApi) {}

  async start() {
    const inviteUserDialog = this.windowsSvc.push(renderInviteUserDialog());
    try {
      while (true) {
        try {
          const inviteUserDialogResult = await inviteUserDialog.interact();
          if (inviteUserDialogResult.choosenUsers) {
            if (inviteUserDialogResult.choosenUsers.length === 0) {
              const infoDialog = this.windowsSvc.push(
                renderSimpleInformation(
                  TR("You have not selected any user.", "no_user_selected")
                )
              );
              await infoDialog.interact();
              infoDialog.close();
            } else {
              // TODO: call api to invite the user.
              const progressDialog = this.windowsSvc.push(
                renderSimpleProgress(T("Working...", "working..."))
              );
              try {
                await this.api.inviteUsers({
                  users: inviteUserDialogResult.choosenUsers.map((user) => ({
                    userId: user.id,
                  })),
                });
              } finally {
                progressDialog.close();
              }
              return;
            }
          } else if (inviteUserDialogResult.isCancel) return;
        } catch (e) {
          console.error(e);
          const errDlg = this.windowsSvc.push(renderErrorDialog(e));
          await errDlg.interact();
          errDlg.close();
        }
      }
    } finally {
      inviteUserDialog.close();
    }
  }
}
