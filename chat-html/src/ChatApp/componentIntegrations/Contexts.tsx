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

import { createContext } from "react";
import { WindowsSvc } from "../components/Windows/WindowsSvc";
import { Chatroom } from "../model/Chatroom";
import { LocalUser } from "../model/LocalUser";
import { Messages } from "../model/Messages";
import { Participants } from "../model/Participants";
import { InviteUserWorkflow } from "../workflows/InviteUserWorkflow";
import { ChatHTTPApi } from "../services/ChatHTTPApi";
import { AbandonChatroomWorkflow } from "../workflows/AbandonChatroomWorkflow";
import { MentionUserWorkflow } from "../workflows/MentionUserWorkflow";
import { RenameChatroomWorkflow } from "../workflows/RenameChatroomWorkflow";

export const CtxWindowsSvc = createContext<WindowsSvc>(null!);
export const CtxLocalUser = createContext<LocalUser>(null!);
export const CtxMessages = createContext<Messages>(null!);
export const CtxChatroom = createContext<Chatroom>(null!);
export const CtxParticipants = createContext<Participants>(null!);
export const CtxAPI = createContext<ChatHTTPApi>(null!);
export const CtxInviteUserWorkflow = createContext<InviteUserWorkflow>(null!);
export const CtxMentionUserWorkflow = createContext<MentionUserWorkflow>(null!);
export const CtxAbandonChatroomWorkflow = createContext<
  AbandonChatroomWorkflow
>(null!);
export const CtxRenameChatroomWorkflow = createContext<RenameChatroomWorkflow>(
  null!
);
