import { Observer } from "mobx-react";
import React, { useContext, useState } from "react";
import CS from "./Dropdown/Dropdown.module.scss";
import cx from "classnames";
import { CtxDropdownRefCtrl } from "./Dropdown/DropdownCommon";
import { CtxDropdownEditor } from "./DropdownEditor";
import { DropdownEditorInput } from "./DropdownEditorInput";
import { Tooltip } from "react-tippy";
import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import { action, observable } from "mobx";
import { createPortal } from "react-dom";
import { DropdownEditorBehavior } from "./DropdownEditorBehavior";

export function TriggerContextMenu(props: { state: TriggerContextMenuState }) {
  return (
    <Observer>
      {() => (
        <>
          {props.state.isDropped
            ? createPortal(
                <div
                  className={"Dropdowner_droppedBox"}
                  style={{ top: props.state.top, left: props.state.left }}
                >
                  <div className="Dropdown_root">
                    <div
                      className={"DropdownItem_root"}
                      onMouseDown={(e) => e.stopPropagation()}
                      onClick={props.state.handleRefreshClick}
                    >
                      Refresh
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
  constructor(public behaviour: DropdownEditorBehavior) {}
  
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
  isInvalid?: boolean;
  invalidMessage?: string;
  backgroundColor?: string;
  foregroundColor?: string;
  customStyle?: any;
}) {
  const ref = useContext(CtxDropdownRefCtrl);
  const beh = useContext(CtxDropdownEditor).behavior;
  const [triggerContextMenu] = useState(() => new TriggerContextMenuState(beh));

  return (
    <Observer>
      {() => (
        <div className={CS.control} ref={ref} onMouseDown={beh.handleControlMouseDown}>
          <DropdownEditorInput
            backgroundColor={props.backgroundColor}
            foregroundColor={props.foregroundColor}
            customStyle={props.customStyle}
          />
          {/*<button className={"inputBtn"} disabled={beh.isReadOnly}>*/}
          {/*  <i className="fas fa-ellipsis-h"></i>*/}
          {/*</button>*/}

          <button
            className={cx("inputBtn", "lastOne", beh.isReadOnly && "readOnly")}
            disabled={beh.isReadOnly}
            tabIndex={-1}
            onClick={!beh.isReadOnly ? beh.handleInputBtnClick : undefined}
            onContextMenu={(event) => {
              beh.handleTriggerContextMenu(event);
              triggerContextMenu.handleTriggerContextMenu(event);
            }}
            onMouseDown={!beh.isReadOnly ? beh.handleControlMouseDown : undefined}
          >
            {!beh.isWorking ? (
              <i className="fas fa-caret-down"></i>
            ) : (
              <i className="fas fa-spinner fa-spin"></i>
            )}
          </button>

          <TriggerContextMenu state={triggerContextMenu} />

          {props.isInvalid && (
            <div className={CS.notification}>
              <Tooltip html={props.invalidMessage} arrow={true}>
                <i className="fas fa-exclamation-circle red" />
              </Tooltip>
            </div>
          )}
        </div>
      )}
    </Observer>
  );
}
