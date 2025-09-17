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

import { flow } from "mobx";
import axios from "axios";
import { IChatTransportOutgoingMessage, IChatTransport, IChatTransportTargert } from "./ChatTransport";

export class ChatTransportPolled implements IChatTransport {
  constructor(public target: IChatTransportTargert) {}

  urlBase = "http://localhost:9099/api";

  receiveMessages() {
    const self = this;
    flow(function* () {
      const response = yield axios.get(`${self.urlBase}/messages`);
      const messages = response.data.map((responseItem: any) => ({
        id: responseItem.id,
        sender: responseItem.sender,
        text: responseItem.text,
        timeSent: responseItem.timeSent,
        avatarUrl: responseItem.avatarUrl,
        type: "message",
      }));
      self.target.realtimeUpdateLog(messages);
    })();
  }

  sendMessage(message: IChatTransportOutgoingMessage): void {
    const self = this;
    flow(function* () {
      const response = yield axios.post(`${self.urlBase}/messages`, message);
    })();
  }

  start() {
    this.receiveMessages();
    setInterval(() => this.receiveMessages(), 1000);
  }
}
