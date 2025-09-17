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

import { renderCreateChatroomDialog } from "../components/Dialogs/CreateChatroomDialog";
import { renderErrorDialog } from "../components/Dialogs/ErrorDialog";
import {
  renderSimpleInformation,
  renderSimpleProgress,
} from "../components/Windows/Windows";
import { WindowsSvc } from "../components/Windows/WindowsSvc";
import { ChatHTTPApi } from "../services/ChatHTTPApi";
import qs from "querystring";
import { T } from "util/translation";

export class CreateChatroomWorkflow {
  constructor(
    public windowsSvc: WindowsSvc,
    public api: ChatHTTPApi,
    public history: any,
    public location: any
  ) {}

  async start(references: { [key: string]: any } | undefined) {
    const createChatroomDialog = this.windowsSvc.push(
      renderCreateChatroomDialog(references)
    );
    try {
      while (true) {
        try {
          const createChatroomDialogResult =
            await createChatroomDialog.interact();
          if(createChatroomDialogResult.isCancel)
          {
            (window.document as any)?.closeOrigamTab?.();
            return;
          }
          if (!createChatroomDialogResult.chatroomTopic) {
            const infoDialog = this.windowsSvc.push(
              renderSimpleInformation(
                T(
                  "You have not entered any topic.",
                  "you_have_not_entered_any_topic"
                )
              )
            );
            await infoDialog.interact();
            infoDialog.close();
            continue;
          }
          if (createChatroomDialogResult.choosenUsers) {
            const progressDialog = this.windowsSvc.push(
              renderSimpleProgress(T("Working...", "working..."))
            );
            try {
              const createChatroomResult = await this.api.createChatroom(
                references || {},
                createChatroomDialogResult.chatroomTopic,
                createChatroomDialogResult.choosenUsers.map((user) => user.id)
              );
              this.history.replace({
                pathname: this.location.pathname,
                search: `?${qs.stringify({
                  chatroomId: createChatroomResult.chatroomId,
                })}`,
              });
              return;
            } finally {
              progressDialog.close();
            }
          }
        } catch (e) {
          console.error(e);
          const errDlg = this.windowsSvc.push(renderErrorDialog(e));
          await errDlg.interact();
          errDlg.close();
        }
      }
    } finally {
      createChatroomDialog.close();
    }
  }
}
