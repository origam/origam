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
import S from "gui/connections/MobileComponents/Form/MobileAction.module.scss";
import { IAction } from "model/entities/types/IAction";
import uiActions from "model/actions-ui-tree";
import { EditLayoutState, ScreenLayoutState } from "model/entities/MobileState/MobileLayoutState";
import { T } from "utils/translation";
import { MobileState } from "model/entities/MobileState/MobileState";
import { ActionList } from "gui/connections/MobileComponents/Form/ActionList";

export const MobileAction: React.FC<{
  action: IAction,
  mobileState: MobileState
}> = (props) => {

  function onClick(event: React.MouseEvent<HTMLButtonElement>) {
    uiActions.actions.onActionClick(props.action)(event, props.action)
  }

  return (
    <button
      className={S.root}
      onClick={onClick}
    >
      {props.action.caption}
    </button>
  );
}

export const MobileActionLink: React.FC<{
  linkAction: IAction,
  actions: IAction[],
  mobileState: MobileState
}> = (props) => {

  function onClick(event: React.MouseEvent<HTMLButtonElement>) {
    props.mobileState.layoutState = new EditLayoutState(
      <ActionList
        actions={props.actions}
        mobileState={props.mobileState}/>,
      props.linkAction.caption,
      new ScreenLayoutState(),
      false,
      true,
    );
  }

  return (
    <button
      className={S.root}
      onClick={onClick}
    >
      {props.linkAction.caption}
    </button>
  );
}

