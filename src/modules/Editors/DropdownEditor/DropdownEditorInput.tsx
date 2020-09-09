import { Observer } from "mobx-react";
import React, { useContext, useEffect, useMemo } from "react";
import { CtxDropdownEditor } from "./DropdownEditor";
import cx from 'classnames';
import S from './DropdownEditor.module.scss';

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
    };
  }, []);

  return (
    <Observer>
      {() => (
        <input
          className={cx("input", S.input)}
          readOnly={beh.isReadOnly}
          ref={refInput}
          placeholder={data.isResolving ? "Loading..." : ""}
          onChange={beh.handleInputChange}
          onKeyDown={!beh.isReadOnly ? beh.handleInputKeyDown : undefined}
          onFocus={!beh.isReadOnly ? beh.handleInputFocus : undefined}
          onBlur={!beh.isReadOnly ? beh.handleInputBlur : undefined}
          onDoubleClick={!beh.isReadOnly ? beh.onDoubleClick : undefined}
          value={beh.inputValue}
          tabIndex={beh.tabIndex ? beh.tabIndex : undefined}
        />
      )}
    </Observer>
  );
}
