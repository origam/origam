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

import { Observer } from "mobx-react";
import React, { useContext, useEffect, useMemo, useState } from "react";
import { CtxDropdownEditor } from "./DropdownEditor";
import cx from 'classnames';
import S from "gui/Components/Dropdown/Dropdown.module.scss";
import { T } from "utils/translation";

export function DropdownEditorInput(props: {
  backgroundColor?: string;
  foregroundColor?: string;
  customStyle?: any;
}) {
  const beh = useContext(CtxDropdownEditor).behavior;
  const data = useContext(CtxDropdownEditor).editorData;
  const setup = useContext(CtxDropdownEditor).setup;
  const [ctrlOrCmdKeyPressed, setCtrlOrCmdKeyPressed] = useState<boolean>(false);
  const isMacOS = () => {return navigator.userAgent.toLowerCase().includes("mac")};

  const refInput = useMemo(() => {
    return (elm: any) => {
      beh.refInputElement(elm);
    };
  }, [beh]);

  useEffect(() => {
    if (beh.subscribeToFocusManager && beh.elmInputElement) {
      beh.subscribeToFocusManager(beh.elmInputElement);
    }
    beh.updateTextOverflowState();
    beh.mount();
    return ()=> beh.onBlur?.(beh.elmInputElement);
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  useEffect(() => {
    beh.updateTextOverflowState();
  }); // eslint-disable-line react-hooks/exhaustive-deps
  function getStyle() {
    if (props.customStyle) {
      return props.customStyle;
    } else {
      return {
        color: props.foregroundColor,
        backgroundColor: props.backgroundColor,
      };
    }
  }

  document.addEventListener('keydown', function(event) {
    if (event.ctrlKey || event.metaKey)
      setCtrlOrCmdKeyPressed(true);
  });
  
  document.addEventListener('keyup', function(event) {
    if (!event.ctrlKey && !event.metaKey)
      setCtrlOrCmdKeyPressed(false);
  });

  const getTitle = () => {
    if (!setup.isLink)
      return "";
    
    else if (isMacOS())
      return T("Hold Cmd and click to open link", "hold_cmd_tool_tip");

    return T("Hold Ctrl and click to open link", "hold_ctrl_tool_tip");
  }

  return (
    <Observer>
      {() => (
        <input
          className={cx("input", S.input, (ctrlOrCmdKeyPressed && setup.isLink) ? ["isLink", S.isLink] : "")}
          title={getTitle()}
          readOnly={beh.isReadOnly}
          ref={refInput}
          placeholder={data.isResolving ? "Loading..." : ""}
          onChange={beh.handleInputChange}
          onKeyDown={beh.handleInputKeyDown}
          onFocus={beh.handleInputFocus}
          onBlur={!beh.isReadOnly ? beh.handleInputBlur : undefined}
          onDoubleClick={event => beh.handleDoubleClick(event)}
          onClick={beh.onClick}
          value={beh.inputValue || ""}
          style={getStyle()}
          autoComplete={"off"}
          autoCorrect={"off"}
          autoCapitalize={"off"}
          spellCheck={"false"}
        />
      )}
    </Observer>
  );
}
