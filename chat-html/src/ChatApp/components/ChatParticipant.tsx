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

import React from "react";
import cx from "classnames";

export enum IChatParticipantStatus {
  Online,
  Offline,
  Away,
  Unknown,
}

export function ChatParticipant(props: {
  avatar: React.ReactNode;
  content: React.ReactNode;
  status?: IChatParticipantStatus;
}) {
  return (
    <div className="chatParticipant">
      <div className="chatParticipant__avatar">
        <div className="avatar">
          <div className="avatar__content">{props.avatar}</div>
          <div
            className={cx("avatar__status", {
              "avatar__status--isOffline":
                props.status === IChatParticipantStatus.Offline,
              "avatar__status--isOnline":
                props.status === IChatParticipantStatus.Online,
              "avatar__status--isAway":
                props.status === IChatParticipantStatus.Away,
            })}
          />
        </div>
      </div>
      <div className="chatParticipant__content">{props.content}</div>
    </div>
  );
}

export function ChatParticipantMini(props: {
  avatar: React.ReactNode;
  name: string;
  status?: IChatParticipantStatus;
}) {
  return (
    <div className="chatParticipantMini" title={props.name}>
      <div className="chatParticipantMini__avatar">
        <div className="avatar">
          <div className="avatar__content">{props.avatar}</div>
          <div
            className={cx("avatar__status", {
              "avatar__status--isOffline":
                props.status === IChatParticipantStatus.Offline,
              "avatar__status--isOnline":
                props.status === IChatParticipantStatus.Online,
              "avatar__status--isAway":
                props.status === IChatParticipantStatus.Away,
            })}
          />
        </div>
      </div>
    </div>
  );
}
