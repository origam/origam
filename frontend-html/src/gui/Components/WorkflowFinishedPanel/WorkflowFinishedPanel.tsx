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

import React, { useEffect, useRef } from "react";
import S from "gui/Components/WorkflowFinishedPanel/WorkflowFinishedPanel.module.scss";
import { T } from "utils/translation";
import cx from "classnames";
import { MultiGrid } from "react-virtualized";
import { requestFocus } from "utils/focus";

export const WorkflowFinishedPanel: React.FC<{
  isCloseButton: boolean;
  isRepeatButton: boolean;
  onCloseClick?(event: any): void;
  onRepeatClick?(event: any): void;
  message: string;
}> = (props) => {
  const repeatButton = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    if (repeatButton.current) {
      requestFocus(repeatButton.current);
    }
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  return (
    <div className={S.root}>
      {props.isRepeatButton && <button ref={repeatButton} onClick={props.onRepeatClick}>{T("Repeat", "button_repeat")}</button>}
      {props.isCloseButton && <button onClick={props.onCloseClick}>{T("Close", "button_close")}</button>}
      <div className={cx(S.message, "workflowMessage")} dangerouslySetInnerHTML={{__html: `${props.message}`}}/>
    </div>)
};
