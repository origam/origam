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

import React, { useContext } from "react";
import { Observer } from "mobx-react";
import {
  MessageCluster,
  IMessageClusterDirection,
} from "../components/MessageCluster";
import { Message } from "../components/Message";
import { MessageHeader } from "../components/MessageHeader";
import moment from "moment";
import { Message as MessageModel } from "../model/Messages";
import { getAvatarUrl } from "../helpers/avatar";
import { CtxMessages, CtxLocalUser } from "./Contexts";

export function ChatFeedUI() {
  const messages = useContext(CtxMessages);
  const localUser = useContext(CtxLocalUser);
  return (
    <Observer>
      {() => {
        function makeMessageDirection(message: MessageModel) {
          if (localUser.id === message.authorId)
            return IMessageClusterDirection.Outbound;
          else return IMessageClusterDirection.Inbound;
        }

        function makeMessageClusters() {
          let clusterNodes: React.ReactNode[] = [];
          let messageNodes: React.ReactNode[] = [];
          let lastMessage: MessageModel | undefined;

          for (let messageItem of messages.items) {
            if (!lastMessage || lastMessage.authorId !== messageItem.authorId) {
              messageNodes = [];
              clusterNodes.push(
                <MessageCluster
                  key={messageItem.id}
                  avatar={
                    <img
                      alt="avatar"
                      className="avatar__picture"
                      src={getAvatarUrl(messageItem.authorAvatarUrl)}
                    />
                  }
                  direction={makeMessageDirection(messageItem)}
                  header={
                    <MessageHeader
                      personName={messageItem.authorName}
                      messageDateTime={moment(messageItem.timeSent).format(
                        "l LTS"
                      )}
                    />
                  }
                  body={<>{messageNodes}</>}
                />
              );
            }
            messageNodes.push(
              <Message
                key={messageItem.id}
                content={
                  <span
                    onClick={(evt) => evt.preventDefault()}
                    className="dangerousContent"
                    dangerouslySetInnerHTML={{ __html: messageItem.text }}
                  />
                }
                isInsertedByClient={messageItem.isLocalOnly}
              />
            );
            lastMessage = messageItem;
          }
          return clusterNodes;
        }

        return <>{makeMessageClusters()}</>;
      }}
    </Observer>
  );
}
