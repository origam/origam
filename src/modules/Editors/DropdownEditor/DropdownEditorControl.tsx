import { Observer } from "mobx-react";
import React, { useContext } from "react";
import CS from "./Dropdown/Dropdown.module.scss";
import cx from "classnames";
import { CtxDropdownRefCtrl } from "./Dropdown/DropdownCommon";
import { CtxDropdownEditor } from "./DropdownEditor";
import { DropdownEditorInput } from "./DropdownEditorInput";

export function DropdownEditorControl() {
  const ref = useContext(CtxDropdownRefCtrl);
  const beh = useContext(CtxDropdownEditor).behavior;
  return (
    <Observer>
      {() => (
        <div className={CS.control} ref={ref} onMouseDown={beh.handleControlMouseDown}>
          <DropdownEditorInput />
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
        </div>
      )}
    </Observer>
  );
}
