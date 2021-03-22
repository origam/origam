import React, { useContext, useEffect, useMemo, useRef } from "react";

import CS from "./CommonStyle.module.css";
import S from "./TagInputEditor.module.css";

import {
  TagInput,
  TagInputAdd,
  TagInputItem,
  TagInputItemDelete,
} from "gui/Components/TagInput/TagInput";
import { inject, observer } from "mobx-react";
import { IProperty } from "model/entities/types/IProperty";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { CtxDropdownEditor } from "../../../../modules/Editors/DropdownEditor/DropdownEditor";
import { CtxDropdownRefCtrl } from "../../../../modules/Editors/DropdownEditor/Dropdown/DropdownCommon";

export const TagInputEditor = inject(({ property }: { property: IProperty }, { value }) => {
  const dataTable = getDataTable(property);
  return {
    textualValues: value?.map((valueItem: any) => dataTable.resolveCellText(property, valueItem)),
  };
})(
  observer(
    (props: {
      value: string[];
      textualValues?: string[];
      isReadOnly: boolean;
      isInvalid: boolean;
      invalidMessage?: string;
      isFocused: boolean;
      backgroundColor?: string;
      foregroundColor?: string;
      customStyle?: any;
      refocuser?: (cb: () => void) => () => void;
      onChange?(event: any, value: string[]): void;
      onKeyDown?(event: any): void;
      onClick?(event: any): void;
      onDoubleClick?(event: any): void;
      onEditorBlur?(event: any): void;
      customInputClass?: string;
    }) => {
      const beh = useContext(CtxDropdownEditor).behavior;
      const ref = useContext(CtxDropdownRefCtrl);
      const data = useContext(CtxDropdownEditor).editorData;

      const value = Array.isArray(props.value) ? [...props.value] : props.value;

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

      function removeItem(event: any, item: string) {
        if (props.isReadOnly) {
          return;
        }
        const index = value.indexOf(item);
        if (index > -1) {
          const removedItem = value.splice(index, 1)[0];
          if (props.onChange) {
            props.onChange(event, value);
          }
          if (props.onEditorBlur) {
            props.onEditorBlur(event);
          }
          data.remove(removedItem);
        }
      }

      const refInput = useMemo(() => {
        return (elm: any) => {
          beh.refInputElement(elm);
        };
      }, []);

      useEffect(() => {
        if (beh.subscribeToFocusManager && beh.elmInputElement) {
          beh.subscribeToFocusManager(beh.elmInputElement);
        }
      }, []);

      const previousValueRef = useRef<string[]>();

      useEffect(() => {
        if (previousValueRef.current?.length !== value?.length) {
          beh.elmInputElement.value = "";
        }
        previousValueRef.current = value;
      }, [previousValueRef.current, previousValueRef.current?.length, value, value?.length]);

      function handleInputKeyDown(event: any) {
        if (event.key === "Backspace" && event.target.value === "" && value.length > 0) {
          removeItem(event, value[value.length - 1]);
        }
        beh.handleInputKeyDown(event);
      }

      return (
        <div className={CS.editorContainer} ref={ref}>
          <TagInput className={S.tagInput}>
            {value
              ? value.map((valueItem, idx) => (
                  <TagInputItem key={valueItem}>
                    <TagInputItemDelete
                      onClick={(event) => {
                        removeItem(event, valueItem);
                      }}
                    />
                    {props.textualValues![idx] || ""}
                  </TagInputItem>
                ))
              : null}
            {props.isReadOnly ? null : (
              <TagInputAdd onClick={(event) => beh.elmInputElement.focus()} />
            )}
            <input
              disabled={props.isReadOnly}
              className={S.filterInput + " " + props.customInputClass}
              ref={refInput}
              placeholder={data.isResolving ? "Loading..." : ""}
              onChange={beh.handleInputChange}
              onKeyDown={handleInputKeyDown}
              onFocus={beh.handleInputFocus}
              onBlur={beh.handleInputBlur}
              onDoubleClick={props.onDoubleClick}
              style={getStyle()}
              size={1}
            />
          </TagInput>
          {props.isInvalid && (
            <div className={CS.notification} title={props.invalidMessage}>
              <i className="fas fa-exclamation-circle red" />
            </div>
          )}
        </div>
      );
    }
  )
);
