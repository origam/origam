import { Observer } from "mobx-react";
import React, {useContext, useEffect, useMemo, useState} from "react";
import { CtxDropdownEditor } from "./DropdownEditor";
import cx from 'classnames';
import S from './DropdownEditor.module.scss';

export function DropdownEditorInput(props:{
  backgroundColor?: string;
  foregroundColor?: string;
  customStyle?: any;
}) {
  const beh = useContext(CtxDropdownEditor).behavior;
  const data = useContext(CtxDropdownEditor).editorData;
  const refInput = useMemo(() => {
    return (elm: any) => {
      beh.refInputElement(elm);
    };
  }, []);
  const [ unsubscribeFromFocusManager, setUnsubscribeFromFocusManager] = useState<() => void>();

  useEffect(() => {
    if(beh.subscribeToFocusManager && beh.elmInputElement){
      setUnsubscribeFromFocusManager(beh.subscribeToFocusManager(beh.elmInputElement));
    }
    return () => {
      beh.unsubscribeFromFocusManager && beh.unsubscribeFromFocusManager();
    };
  }, []);

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
          onDoubleClick={beh.onDoubleClick}
          value={beh.inputValue}
          style={getStyle()}
        />
      )}
    </Observer>
  );
}
