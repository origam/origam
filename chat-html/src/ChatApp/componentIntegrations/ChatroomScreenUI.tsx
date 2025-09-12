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

import { autorun } from "mobx";
import { Observer } from "mobx-react";
import React, { useContext, useEffect, useRef, useState } from "react";
import { buildReferenceLink } from "../../util/links";
import {
  ChatParticipantMini,
  IChatParticipantStatus,
} from "../components/ChatParticipant";
import { MessageBar } from "../components/MessageBar";
import { getAvatarUrl } from "../helpers/avatar";
import { IParticipantStatus } from "../model/Participants";
import { ChatFeedUI } from "./ChatFeedUI";
import {
  CtxAbandonChatroomWorkflow,
  CtxAPI,
  CtxChatroom,
  CtxInviteUserWorkflow,
  CtxMessages,
  CtxParticipants,
  CtxRenameChatroomWorkflow,
} from "./Contexts";
import { SendMessageBarUI } from "./SendMessageBarUI";
import cx from "classnames";

export function ChatroomName(props: { value: string }) {
  const renameChatroomWorkflow = useContext(CtxRenameChatroomWorkflow);
  return (
    <div className="messageThreadHeader__title">
      <h1 onClick={() => renameChatroomWorkflow.start()}>
        {props.value || <>&nbsp;</>}
        <div className="messageThreadHeader__editIcon">
          <i className="fas fa-edit fa-xs" />
        </div>
      </h1>
    </div>
  );
}

export function ChatroomHashtag(props: {
  categoryId: string | null;
  referenceId: string | null;
}) {
  const api = useContext(CtxAPI);
  const [linkText, setLinkText] = useState("");
  useEffect(() => {
    if (props.categoryId && props.referenceId) {
      api
        .getHashtagLabels(props.categoryId, [props.referenceId])
        .then((result) => {
          setLinkText(`${props.categoryId} / ${result[props.referenceId!]}`);
        });
    }
  }, [props.categoryId, props.referenceId, api]);
  if (!props.categoryId || !props.referenceId) return null;
  return (
    <div className="chatroomHashtag">
      <a href={buildReferenceLink(props.categoryId, props.referenceId)}>
        {linkText}
      </a>
    </div>
  );
}

export function ChatroomScreenUI(props: { isBlur?: boolean }) {
  const refMessageBar = useRef<any>();
  const messages = useContext(CtxMessages);

  const [isFollowingTail, setFollowingTail] = useState(true);

  useEffect(() => {
    let prevMsgCount: number | undefined;
    return autorun(() => {
      if (
        refMessageBar.current &&
        (prevMsgCount === 0 || prevMsgCount === undefined) &&
        messages.items.length !== 0
      ) {
        // 1s should be enough to be sure that the component has been already laid out
        // Otherwise this autorun gets called before the components observer before
        // the scrolled block has been filled in with messages.
        setTimeout(() => {
          refMessageBar.current?.scrollToEnd();
        }, 1000);
      }
      prevMsgCount = messages.items.length;
    });
  }, [messages.items.length]);

  function handleScrolledToTail(isTailed: boolean) {
    setFollowingTail(isTailed);
  }

  const chatroom = useContext(CtxChatroom);
  const participants = useContext(CtxParticipants);
  const inviteUserWorkflow = useContext(CtxInviteUserWorkflow);
  const abandonChatroomWorkflow = useContext(CtxAbandonChatroomWorkflow);


  function makeParticipantStatus(statusIn: IParticipantStatus) {
    switch (statusIn) {
      case IParticipantStatus.Online:
        return IChatParticipantStatus.Online;
      case IParticipantStatus.Away:
        return IChatParticipantStatus.Away;
      case IParticipantStatus.Offline:
        return IChatParticipantStatus.Offline;
      default:
        return IChatParticipantStatus.Unknown;
    }
  }

  return (
    <Observer>
      {() => {
        const participantItems = participants.items;
        const chatroomName = chatroom.topic;
        const chatroomReferenceId = chatroom.referenceId;
        const chatroomCategoryId = chatroom.categoryId;

        return (
          <div className={cx("App", { isBlur: props.isBlur })}>
            {/*<div className="sidebarArea">
              <Sidebar>
                <SidebarRow>
                  <div
                    className="addUserSidebarItem"
                    onClick={() => setIsInviteUserModalShown(true)}
                  >
                    <div className="addUserSidebarItemIcon">
                      <i className="fas fa-plus-circle" />
                    </div>
                    <div className="addUserSidebarItemContent">
                      Invite user...
                    </div>
                  </div>
                </SidebarRow>
                <ChatParticipantsUI />
              </Sidebar>
            </div>*/}
            <div className="messageArea">
              <div className="messageThreadHeader">
                <div className="messageThreadHeader__info">
                  <ChatroomName value={chatroomName} />
                  <ChatroomHashtag
                    categoryId={chatroomCategoryId}
                    referenceId={chatroomReferenceId}
                  />
                </div>
                <div className="messageThreadHeader__actions">
                  {participantItems.map((item) => (
                    <ChatParticipantMini
                      key={item.id}
                      avatar={
                        <img
                          alt=""
                          className="avatar__picture"
                          src={getAvatarUrl(item.avatarUrl)}
                        />
                      }
                      name={item.name}
                      status={makeParticipantStatus(item.status)}
                    />
                  ))}

                  <div
                    className="messageThreadHeader__actionButton"
                    onClick={() => inviteUserWorkflow.start()}
                  >
                    <i className="fas fa-user-plus" />
                  </div>
                  <div
                    className="messageThreadHeader__actionButton"
                    onClick={() => abandonChatroomWorkflow.start()}
                  >
                    <i className="fas fa-user-minus" />
                  </div>
                </div>
              </div>
              <MessageBar
                ref={refMessageBar}
                messages={<ChatFeedUI />}
                onUserScrolledToTail={handleScrolledToTail}
                isTrackingLatestMessages={isFollowingTail}
              />
              <SendMessageBarUI
                onMessageWillSend={() => {
                  setFollowingTail(true);
                  refMessageBar.current?.scrollToEnd();
                }}
              />
            </div>

            {/*isInviteUserModalShown && (
              <InviteUserModal
                onCloseClick={() => setIsInviteUserModalShown(false)}
              />
            )*/}
          </div>
        );
      }}
    </Observer>
  );
}
