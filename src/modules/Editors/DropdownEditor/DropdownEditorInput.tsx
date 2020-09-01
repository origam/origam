import { Observer } from "mobx-react";
import React, {useContext, useEffect, useMemo } from "react";
import { CtxDropdownEditor } from "./DropdownEditor";

export function DropdownEditorInput() {
  const beh = useContext(CtxDropdownEditor).behavior;
  const data = useContext(CtxDropdownEditor).editorData;
  const refInput = useMemo(() => {
    return (elm: any) => {
      beh.refInputElement(elm);
    };
  }, []);

  useEffect(() => {
    beh.elmInputElement.focus();
    return () => {
      beh.unsubscribeFromFocusManager && beh.unsubscribeFromFocusManager();
    }
  }, [])

  return (
    <Observer>
      {() => (
        <input
          className={"input"}
          readOnly={beh.isReadOnly}
          ref={refInput}
          placeholder={data.isResolving ? "Loading..." : ""}
          onChange={beh.handleInputChange}
          onKeyDown={beh.handleInputKeyDown}
          onFocus={beh.handleInputFocus}
          onBlur={beh.handleInputBlur}
          value={beh.inputValue}
          tabIndex={beh.tabIndex ? beh.tabIndex : undefined}
          onDoubleClick={beh.onDoubleClick}
        />
      )}
    </Observer>
  );
}
