import { Observer } from "mobx-react";
import React, { useContext } from "react";
import S from "./Dropdown/Dropdown.module.scss";
import { CtxDropdownRefCtrl } from "./Dropdown/DropdownCommon";
import { CtxDropdownEditor } from "./DropdownEditor";
import { DropdownEditorInput } from "./DropdownEditorInput";

export function DropdownEditorControl() {
  const ref = useContext(CtxDropdownRefCtrl);
  const beh = useContext(CtxDropdownEditor).behavior;
  return (
    <Observer>
      {() => (
        <div className={S.control} ref={ref} onMouseDown={beh.handleControlMouseDown}>
          <DropdownEditorInput />
          {/*<button className={"inputBtn"} disabled={beh.isReadOnly}>*/}
          {/*  <i className="fas fa-ellipsis-h"></i>*/}
          {/*</button>*/}
          <button className={"inputBtn lastOne"}
                  disabled={beh.isReadOnly}
                  tabIndex={-1}
                  onClick={beh.handleInputBtnClick}>
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
