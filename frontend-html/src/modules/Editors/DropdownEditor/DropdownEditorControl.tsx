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
import React, { useContext, useEffect, useState } from "react";
import CS from "gui/Components/Dropdown/Dropdown.module.scss";
import cx from "classnames";
import { CtxDropdownEditor } from "./DropdownEditor";
import { DropdownEditorInput } from "./DropdownEditorInput";
import { action, observable } from "mobx";
import { createPortal } from "react-dom";
import { DropdownEditorBehavior } from "./DropdownEditorBehavior";
import { CtxDropdownRefCtrl } from "gui/Components/Dropdown/DropdownCommon";

export function TriggerContextMenu(props: { state: TriggerContextMenuState }) {
  return (
    <Observer>
      {() => (
        <>
          {props.state.isDropped
            ? createPortal(
              <div
                className={"Dropdowner_droppedBox"}
                style={{top: props.state.top, left: props.state.left}}
              >
                <div className="Dropdown_root">
                  <div
                    className={"DropdownItem_root"}
                    onMouseDown={(e) => e.stopPropagation()}
                    onClick={props.state.handleRefreshClick}
                  >
                    {T("Refresh", "refresh_dropdown")}
                  </div>
                </div>
              </div>,
              document.getElementById("dropdown-portal")!
            )
            : null}
        </>
      )}
    </Observer>
  );
}

class TriggerContextMenuState {
  constructor(public behaviour: DropdownEditorBehavior) {
  }

  @observable isDropped = false;
  @observable top = 0;
  @observable left = 0;

  @action.bound
  handleTriggerContextMenu(event: any) {
    event.preventDefault();
    if (!this.isDropped) {
      this.top = event.clientY;
      this.left = event.clientX;
      this.isDropped = true;
      window.addEventListener("mousedown", this.handleScreenMouseDown);
    } else {
      this.isDropped = false;
      window.removeEventListener("mousedown", this.handleScreenMouseDown);
    }
  }

  @action.bound
  handleScreenContextMenu(event: any) {
    event.preventDefault();
    this.isDropped = false;
  }

  @action.bound
  handleScreenMouseDown(event: any) {
    event.preventDefault();
    this.isDropped = false;
  }

  @action.bound
  handleRefreshClick(event: any) {
    this.isDropped = false;
    this.behaviour.clearCache();
  }
}

export function DropdownEditorControl(props: {
  backgroundColor?: string;
  foregroundColor?: string;
  customStyle?: any;
}) {
  const ref = useContext(CtxDropdownRefCtrl);
  const beh = useContext(CtxDropdownEditor).behavior;
  const [triggerContextMenu] = useState(() => new TriggerContextMenuState(beh));
  useEffect(()=> beh.onEditorMounted(), []);

  return (
    <Observer>
      {() => (
        <div className={CS.control} ref={ref} onMouseDown={beh.handleControlMouseDown}>
          <DropdownEditorInput
            backgroundColor={props.backgroundColor}
            foregroundColor={props.foregroundColor}
            customStyle={props.customStyle}
          />
          {!beh.isReadOnly && beh.hasNewScreenButton
             && <div
              className={"inputBtn"}
              onClick={() => beh.onAddNewRecordClick?.(beh.userEnteredValue)}
            >
              +
            </div>
          }
          {!beh.isReadOnly
            && <div
              className={cx("inputBtn", "lastOne", beh.isReadOnly && "readOnly")}
              tabIndex={-1}
              onClick={!beh.isReadOnly ? beh.handleInputBtnClick : undefined}
              onContextMenu={(event) => {
                beh.handleTriggerContextMenu(event);
                triggerContextMenu.handleTriggerContextMenu(event);
              }}
              onMouseDown={!beh.isReadOnly ? beh.handleControlMouseDown : undefined}
            >
              {!beh.isWorking ? (
                <i className="fas fa-caret-down"/>
              ) : (
                <i className="fas fa-spinner fa-spin"/>
              )}
            </div>}

          <TriggerContextMenu state={triggerContextMenu}/>
        </div>
      )}
    </Observer>
  );
}
