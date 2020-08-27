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
          disabled={beh.isReadOnly}
          ref={refInput}
          placeholder={data.isResolving ? "Loading..." : ""}
          onChange={beh.handleInputChange}
          onKeyDown={beh.handleInputKeyDown}
          onFocus={beh.handleInputFocus}
          onBlur={beh.handleInputBlur}
          value={beh.inputValue}
          onDoubleClick={beh.onDoubleClick}
        />
      )}
    </Observer>
  );
}
