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

export enum IMessageClusterDirection {
  Inbound = "I",
  Outbound = "O",
}

export function MessageCluster(props: {
  avatar: React.ReactNode;
  header: React.ReactNode;
  body: React.ReactNode;
  direction: IMessageClusterDirection;
}) {
  return (
    <div
      className={cx("messageCluster", {
        "messageCluster--inbound": props.direction === IMessageClusterDirection.Inbound,
        "messageCluster--outbound": props.direction === IMessageClusterDirection.Outbound,
      })}
    >
      <div className="messageCluster__avatarSection">
        <div className="avatar">
          <div className="avatar__content">{props.avatar}</div>
        </div>
      </div>
      <div className="messageCluster__contentSection">
        <div className="messageCluster__header">{props.header}</div>
        <div className="messageCluster__body">{props.body}</div>
      </div>
    </div>
  );
}
