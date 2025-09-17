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

import { delay } from "../../util/delay";
import { WindowsSvc } from "../components/Windows/WindowsSvc";
import { renderErrorDialog } from "../components/Dialogs/ErrorDialog";
import { Messages } from "../model/Messages";
import { ChatHTTPApi } from "./ChatHTTPApi";
import { Chatroom } from "../model/Chatroom";
import { Participants, IParticipantStatus } from "../model/Participants";
import { LocalUser } from "../model/LocalUser";
import { TR } from "util/translation";

export class TransportSvc {
  constructor(
    public windowSvc: WindowsSvc,
    public messages: Messages,
    public chatroom: Chatroom,
    public participants: Participants,
    public localUser: LocalUser,
    public api: ChatHTTPApi
  ) {}

  pollingIntervalMs = 10000;
  isTerminated = false;

  async realoadPolledData() {
    this.messages.clear();
    await this.loadPolledData();
  }

  async initialLoadPolledData() {
    try {
      await this.loadPolledData();
    } catch (e) {
      console.error(e);
      const errDlg = this.windowSvc.push(renderErrorDialog(e));
      await errDlg.interact();
      errDlg.close();
    }
  }

  async loadPolledDataRecentMessagesOnly() {
    const lastMessage = this.messages.lastServerMessage;
    await this.loadPolledData(lastMessage && lastMessage.id);
  }

  async runLoop() {
    let isDelay = true;
    while (!this.isTerminated) {
      if (isDelay) await delay(this.pollingIntervalMs);
      if (this.isTerminated) return;
      isDelay = true;
      try {
        await this.loadPolledDataRecentMessagesOnly();
      } catch (e) {
        console.error(e);
        isDelay = false;
        const errDlg = this.windowSvc.push(renderErrorDialog(e));
        await errDlg.interact();
        errDlg.close();
      }
    }
  }

  terminateLoop() {
    this.isTerminated = true;
  }

  async loadPolledData(afterIdIncluding?: string) {
    const polledData = await this.api.getPolledData(afterIdIncluding);

    this.localUser.name = polledData.localUser.name;
    this.localUser.id = polledData.localUser.id;
    this.localUser.avatarUrl = polledData.localUser.avatarUrl;

    this.chatroom.topic = polledData.info.topic;
    document.title = polledData.info.topic
      ? `${TR("Chat", "Chat")}: ${polledData.info.topic}`
      : TR("Chat (no topic)", "Chat_no_topic");
      this.chatroom.categoryId = polledData.info.categoryName;
      this.chatroom.referenceId = polledData.info.referenceId;

    this.messages.mergeMessages({ messages: polledData.messages });

    this.participants.setItems({
      participants: polledData.participants.map((participant) => ({
        ...participant,
        status: transformStatusIn(participant.status),
      })),
    });
  }
}

function transformStatusIn(status: "online" | "away" | "offline" | "none") {
  switch (status) {
    case "away":
      return IParticipantStatus.Away;
    case "online":
      return IParticipantStatus.Online;
    case "offline":
      return IParticipantStatus.Offline;
    case "none":
      return IParticipantStatus.Unknown;
  }
}
