import { Observer } from "mobx-react";
import React, { useContext } from "react";
import CS from "./Dropdown/Dropdown.module.scss";
import cx from "classnames";
import { CtxDropdownRefCtrl } from "./Dropdown/DropdownCommon";
import { CtxDropdownEditor } from "./DropdownEditor";
import { DropdownEditorInput } from "./DropdownEditorInput";
import { Tooltip } from "react-tippy";

export function DropdownEditorControl(props: {
  isInvalid?: boolean;
  invalidMessage?: string;
  backgroundColor?: string;
  foregroundColor?: string;
  customStyle?: any;
}) {
  const ref = useContext(CtxDropdownRefCtrl);
  const beh = useContext(CtxDropdownEditor).behavior;
  return (
    <Observer>
      {() => (
        <div className={CS.control} ref={ref} onMouseDown={beh.handleControlMouseDown}>
          <DropdownEditorInput
            backgroundColor={props.backgroundColor}
            foregroundColor={props.foregroundColor}
            customStyle={props.customStyle}/>
          {/*<button className={"inputBtn"} disabled={beh.isReadOnly}>*/}
          {/*  <i className="fas fa-ellipsis-h"></i>*/}
          {/*</button>*/}
          <button
            className={cx("inputBtn", "lastOne", beh.isReadOnly && "readOnly")}
            disabled={beh.isReadOnly}
            tabIndex={-1}
            onClick={!beh.isReadOnly ? beh.handleInputBtnClick : undefined}
            onMouseDown={!beh.isReadOnly ? beh.handleControlMouseDown : undefined}
          >
            {!beh.isWorking ? (
              <i className="fas fa-caret-down"></i>
            ) : (
              <i className="fas fa-spinner fa-spin"></i>
            )}
          </button>
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
