import { Observer } from "mobx-react";
import React, { useContext, useMemo } from "react";
import { CtxDropdownEditor } from "./DropdownEditor";

export function DropdownEditorInput() {
  const beh = useContext(CtxDropdownEditor).behavior;
  const data = useContext(CtxDropdownEditor).editorData;
  const refInput = useMemo(() => {
    return (elm: any) => {
      beh.refInputElement(elm);
    };
  }, []);
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
          tabIndex={beh.tabIndex ? beh.tabIndex : undefined}
          onDoubleClick={beh.onDoubleClick}
        />
      )}
    </Observer>
  );
}
