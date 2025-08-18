/*
Copyright 2005 - 2024 Advantage Solutions, s. r. o.

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

import React, { useEffect, useRef } from "react";
import { getFormFocusManager } from "model/selectors/DataView/getFormFocusManager";
import { getDataView } from "model/selectors/DataView/getDataView";
import uiActions from "model/actions-ui-tree";
import S from "gui/Workbench/ScreenArea/FormView/BackupFocusPlaceHolder.module.scss";


// This components should get focus if there are no other focusable inputs on the screen section.
// This will ensure that the focus does not stay where we don't expect it to be. For example on
// the parent screen after opening a model window. 
export const BackupFocusPlaceHolder = (props: { ctx: any; }) => {
  const refInput = useRef<HTMLInputElement>(null);
  useEffect(() => {
    const focusManager = getFormFocusManager(getDataView(props.ctx));
    focusManager.setBackupFocusPlaceHolder(refInput.current);
  }, []);

  return (
    <input
      className={S.root}
      onKeyDown={(event) => {
        if (event.key === "Enter") {
          const dataView = getDataView(props.ctx);
          if (dataView.firstEnabledDefaultAction) {
            uiActions.actions.onActionClick(dataView.firstEnabledDefaultAction)(
              event,
              dataView.firstEnabledDefaultAction
            );
          }
        }
      }}
      ref={refInput} />);
};
