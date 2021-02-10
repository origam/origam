import { Observer } from "mobx-react";
import React, {useContext, useEffect, useMemo} from "react";
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
  const setup = useContext(CtxDropdownEditor).setup;
  const refInput = useMemo(() => {
    return (elm: any) => {
      beh.refInputElement(elm);
    };
  }, []);

  useEffect(() => {
    if(beh.subscribeToFocusManager && beh.elmInputElement){
      beh.subscribeToFocusManager(beh.elmInputElement);
    }
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
          className={cx("input", S.input, {isLink: setup.isLink})}
          readOnly={beh.isReadOnly}
          ref={refInput}
          placeholder={data.isResolving ? "Loading..." : ""}
          onChange={beh.handleInputChange}
          onKeyDown={beh.handleInputKeyDown}
          onFocus={!beh.isReadOnly ? beh.handleInputFocus : undefined}
          onBlur={!beh.isReadOnly ? beh.handleInputBlur : undefined}
          onDoubleClick={beh.onDoubleClick}
          onClick={beh.onClick}
          value={beh.inputValue}
          style={getStyle()}
        />
      )}
    </Observer>
  );
}
