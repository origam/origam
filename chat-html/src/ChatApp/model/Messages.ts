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

import { observable, decorate, action, computed } from "mobx";

export class Mention {
  constructor(
    public id: string = "",
    public name: string = "",
    public avatarUrl: string = ""
  ) {}
}

export class Message {
  constructor(
    public id: string = "",
    public authorId: string = "",
    public authorName: string = "",
    public authorAvatarUrl: string = "",
    public timeSent: string = "",
    public text: string = "",
    public mentions: Mention[] = [],
    public isLocalOnly: boolean = false
  ) {}
}

decorate(Message, {
  id: observable,
  timeSent: observable,
  authorName: observable,
  isLocalOnly: observable,
});

export interface IMergeMessagesInput {
  messages: {
    id: string;
    newId?: string;
    authorId: string;
    authorName: string;
    authorAvatarUrl: string;
    timeSent: string;
    text: string;
    mentions: {
      id: string;
      name: string;
      avatarUrl: string;
    }[];
  }[];
}

export interface IPushLocalMessageArg {
  id: string;
  authorId: string;
  authorAvatarUrl: string;
  authorName: string;
  timeSent: string;
  text: string;
  mentions: {
    id: string;
    name: string;
    avatarUrl: string;
  }[];
}

export class Messages {
  @observable items: Message[] = [];

  @action.bound
  mergeMessages(arg: IMergeMessagesInput) {
    const localMessageMap = new Map(this.items.map((item) => [item.id, item]));
    const idsProcessed = new Set<string>();
    for (let msgIn of arg.messages) {
      const localMsg = localMessageMap.get(msgIn.id);
      if (localMsg) {
        if (msgIn.newId) localMsg.id = msgIn.id;
        localMsg.timeSent = msgIn.timeSent;
        localMsg.authorName = msgIn.authorName;
        localMsg.authorAvatarUrl = msgIn.authorAvatarUrl;
        localMsg.text = msgIn.text;
        localMsg.isLocalOnly = false;
        idsProcessed.add(msgIn.id);
      }
    }
    for (let msgIn of arg.messages) {
      if (idsProcessed.has(msgIn.id)) continue;
      this.items.push(
        new Message(
          msgIn.id,
          msgIn.authorId,
          msgIn.authorName,
          msgIn.authorAvatarUrl,
          msgIn.timeSent,
          msgIn.text,
          msgIn.mentions.map(
            (mention) =>
              new Mention(mention.id, mention.name, mention.avatarUrl)
          )
        )
      );
    }
  }

  @action.bound
  pushLocalMessage(arg: IPushLocalMessageArg) {
    this.items.push(
      new Message(
        arg.id,
        arg.authorId,
        arg.authorName,
        arg.authorAvatarUrl,
        arg.timeSent,
        arg.text,
        [],
        true
      )
    );
  }

  @action.bound clear() {
    this.items.length = 0;
  }

  @computed get lastServerMessage() {
    let items = [...this.items];
    items.reverse();
    return items.find((item) => !item.isLocalOnly);
  }
}
