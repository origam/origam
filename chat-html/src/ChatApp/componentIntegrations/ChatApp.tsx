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

import React, { useEffect, useState } from "react";
import qs from "querystring";
import { WindowsSvc } from "../components/Windows/WindowsSvc";
import { Chatroom } from "../model/Chatroom";
import { LocalUser } from "../model/LocalUser";
import { Messages } from "../model/Messages";
import { Participants } from "../model/Participants";
import { ChatHTTPApi } from "../services/ChatHTTPApi";
import { TransportSvc } from "../services/TransportSvc";
import { InviteUserWorkflow } from "../workflows/InviteUserWorkflow";
import { ChatroomScreenUI } from "./ChatroomScreenUI";
import {
  CtxAPI,
  CtxChatroom,
  CtxInviteUserWorkflow,
  CtxLocalUser,
  CtxMessages,
  CtxParticipants,
  CtxWindowsSvc,
  CtxAbandonChatroomWorkflow,
  CtxMentionUserWorkflow,
  CtxRenameChatroomWorkflow,
} from "./Contexts";
import { useLocation, useHistory } from "react-router";
import { AbandonChatroomWorkflow } from "../workflows/AbandonChatroomWorkflow";
import { MentionUserWorkflow } from "../workflows/MentionUserWorkflow";
import { CreateChatroomWorkflow } from "../workflows/CreateChatroomWorkflow";
import { RenameChatroomWorkflow } from "../workflows/RenameChatroomWorkflow";
import { HashtagRootStore } from "../modules/hashtagging/stores/RootStore";
import { CtxHashtagRootStore } from "../modules/hashtagging/components/Common";
import { populateHashtaggingStore } from "../modules/hashtagging/HashtaggingApp";
import { Observer } from "mobx-react";

function ctxProvide<T>(node: React.ReactNode, Ctx: React.Context<T>, value: T) {
  return <Ctx.Provider value={value}>{node}</Ctx.Provider>;
}

export function ChatApp() {
  const location = useLocation();
  const history = useHistory();
  const locationQuery = qs.parse(location.search.slice(1));
  const references = Object.fromEntries(
    Object.entries(locationQuery).filter(([k, v]) => k.startsWith("reference"))
  );

  const chatroomId = locationQuery.chatroomId as string;
  const fakeUserId = locationQuery.fakeUserId as string;

  const [isTerminated, setIsTerminated] = useState(false);

  const [services] = useState(() => {
    const windowsSvc = new WindowsSvc();
    const api = new ChatHTTPApi(chatroomId, fakeUserId);

    const localUser = new LocalUser();
    const chatroom = new Chatroom();
    const messages = new Messages();
    const participants = new Participants();

    const transportSvc = new TransportSvc(
      windowsSvc,
      messages,
      chatroom,
      participants,
      localUser,
      api
    );

    const inviteUserWorkflow = new InviteUserWorkflow(windowsSvc, api);
    const mentionUserWorkflow = new MentionUserWorkflow(windowsSvc, api);
    const abandonChatroomWorkflow = new AbandonChatroomWorkflow(
      windowsSvc,
      transportSvc,
      () => setIsTerminated(true),
      api,
      localUser
    );
    const createChatroomWorkflow = new CreateChatroomWorkflow(
      windowsSvc,
      api,
      history,
      location
    );

    const renameChatroomWorkflow = new RenameChatroomWorkflow(
      windowsSvc,
      api,
      transportSvc
    );

    const hashtagRootStore = new HashtagRootStore(windowsSvc, api);
    populateHashtaggingStore(hashtagRootStore);

    return {
      windowsSvc,
      localUser,
      chatroom,
      participants,
      messages,
      transportSvc,
      hashtagRootStore,
      inviteUserWorkflow,
      mentionUserWorkflow,
      abandonChatroomWorkflow,
      createChatroomWorkflow,
      renameChatroomWorkflow,
      api,
    };
  });

  useEffect(() => {
    if (chatroomId) {
      services.api.setChatroomId(chatroomId);
      services.transportSvc.initialLoadPolledData();
      services.transportSvc.runLoop();
      return () => {
        services.transportSvc.terminateLoop();
      };
    } else {
      services.createChatroomWorkflow.start(references);
    }
  }, [
    chatroomId,
    references,
    services.api,
    services.createChatroomWorkflow,
    services.transportSvc,
  ]);

  let uiTree = (
    <Observer>
      {() => (
        <>
          {!isTerminated && chatroomId && (
            <ChatroomScreenUI isBlur={services.windowsSvc.displaysWindow} />
          )}
          {services.windowsSvc.renderStack()}
        </>
      )}
    </Observer>
  );
  uiTree = ctxProvide(uiTree, CtxWindowsSvc, services.windowsSvc);
  uiTree = ctxProvide(uiTree, CtxMessages, services.messages);
  uiTree = ctxProvide(uiTree, CtxLocalUser, services.localUser);
  uiTree = ctxProvide(uiTree, CtxParticipants, services.participants);
  uiTree = ctxProvide(uiTree, CtxChatroom, services.chatroom);
  uiTree = ctxProvide(uiTree, CtxAPI, services.api);
  uiTree = ctxProvide(
    uiTree,
    CtxInviteUserWorkflow,
    services.inviteUserWorkflow
  );
  uiTree = ctxProvide(
    uiTree,
    CtxMentionUserWorkflow,
    services.mentionUserWorkflow
  );
  uiTree = ctxProvide(
    uiTree,
    CtxAbandonChatroomWorkflow,
    services.abandonChatroomWorkflow
  );
  uiTree = ctxProvide(
    uiTree,
    CtxRenameChatroomWorkflow,
    services.renameChatroomWorkflow
  );
  uiTree = ctxProvide(uiTree, CtxHashtagRootStore, services.hashtagRootStore);

  return <>{uiTree}</>;
}
