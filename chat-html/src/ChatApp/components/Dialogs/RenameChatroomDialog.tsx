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

import { action, observable } from "mobx";
import { Observer } from "mobx-react";
import React, { useContext, useState } from "react";
import { T, TR } from "util/translation";
import { CtxAPI, CtxWindowsSvc } from "../../componentIntegrations/Contexts";
import { ChatHTTPApi } from "../../services/ChatHTTPApi";
import { Button } from "../Buttons";
import {
  DefaultModal,
  ModalCloseButton,
  ModalFooter,
} from "../Windows/Windows";
import { IModalHandle, WindowsSvc } from "../Windows/WindowsSvc";

export interface IInteractor {
  chatroomTopic?: string;
  isCancel?: boolean;
}

class DialogState {
  constructor(public api: ChatHTTPApi, public windowsSvc: WindowsSvc) {}

  @observable chatroomTopic: string = "";

  @action.bound
  handleChatroomTopicChange(event: any) {
    this.chatroomTopic = event.target.value;
  }
}

export function RenameChatroomDialog(props: {
  onCancel?: any;
  onSubmit?: (chatroomTopic: string) => void;
}) {
  const api = useContext(CtxAPI);
  const windowsSvc = useContext(CtxWindowsSvc);
  const [state] = useState(() => new DialogState(api, windowsSvc));
  return (
    <DefaultModal
      footer={
        <ModalFooter align="center">
          <Button onClick={() => props.onSubmit?.(state.chatroomTopic)}>
            {T("Ok", "Ok")}
          </Button>
          <Button onClick={() => props.onCancel?.(state.chatroomTopic)}>
            {T("Cancel", "Cancel")}
          </Button>
        </ModalFooter>
      }
    >
      <ModalCloseButton onClick={props.onCancel} />
      <div className="renameChatroomModalContent">
        <div className="chooseUserToInviteModalContent__header">
          <p>
            {T(
              "Enter new topic for this chatroom:",
              "enter_chatroom_new_topic"
            )}
          </p>
          <Observer>
            {() => (
              <input
                value={state.chatroomTopic}
                onChange={state.handleChatroomTopicChange}
                className="chatroomTopicInput"
                placeholder={TR("New name", "new_name")}
              />
            )}
          </Observer>
        </div>
      </div>
    </DefaultModal>
  );
}

export function renderRenameChatroomDialog() {
  return (modal: IModalHandle<IInteractor>) => {
    return (
      <RenameChatroomDialog
        onCancel={() => modal.resolveInteract({ isCancel: true })}
        onSubmit={(chatroomTopic) => modal.resolveInteract({ chatroomTopic })}
      />
    );
  };
}
