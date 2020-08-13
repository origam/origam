import React, { useContext, useEffect, useMemo, useRef } from "react";
import { Tooltip } from "react-tippy";

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

export const TagInputEditor = inject(({ property }: { property: IProperty }, { value }) => {
  const dataTable = getDataTable(property);
  return {
    textualValues: dataTable.resolveCellText(property, value),
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
      refocuser?: (cb: () => void) => () => void;
      onChange?(event: any, value: any): void;
      onKeyDown?(event: any): void;
      onClick?(event: any): void;
      onEditorBlur?(event: any): void;
    }) => {
      function removeItem(event: any, item: string) {
        const index = props.value.indexOf(item);
        if (index > -1) {
          props.value.splice(index, 1);
          if (props.onChange) {
            props.onChange(event, props.value);
          }
          if (props.onEditorBlur) {
            props.onEditorBlur(event);
          }
        }
      }

      const beh = useContext(CtxDropdownEditor).behavior;
      const data = useContext(CtxDropdownEditor).editorData;
      const refInput = useMemo(() => {
        return (elm: any) => {
          beh.refInputElement(elm);
        };
      }, []);

      const previousValueRef = useRef<string[]>();

      useEffect(() => {
        if(previousValueRef.current !== undefined &&
          previousValueRef.current.length !== props.value.length){
          beh.elmInputElement.value = "";
        }
        previousValueRef.current = props.value;
      });

      return (
        <div className={CS.editorContainer}>
          <TagInput className={S.tagInput}>
            {props.value
              ? props.value.map((valueItem, idx) => (
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
            <TagInputAdd onClick={(event) => beh.elmInputElement.focus()} />
            <input
              ref={refInput}
              placeholder={data.isResolving ? "Loading..." : ""}
              onChange={beh.handleInputChange}
              onKeyDown={beh.handleInputKeyDown}
              onFocus={beh.handleInputFocus}
              onBlur={beh.handleInputBlur}
              // value={beh.inputValue}
            />
          </TagInput>
          {/* <input
        style={{
          color: this.props.foregroundColor,
          backgroundColor: this.props.backgroundColor
        }}
        className={CS.editor}
        type="text"
        value={this.props.value || ""}
        readOnly={true || this.props.isReadOnly}
        // ref={this.refInput}
        onChange={(event: any) =>
          this.props.onChange &&
          this.props.onChange(event, event.target.value)
        }
        onKeyDown={this.props.onKeyDown}
        onClick={this.props.onClick}
        onBlur={this.props.onEditorBlur}
        // onFocus={this.handleFocus}
      />*/}
          {props.isInvalid && (
            <div className={CS.notification}>
              <Tooltip html={props.invalidMessage} arrow={true}>
                <i className="fas fa-exclamation-circle red" />
              </Tooltip>
            </div>
          )}
        </div>
      );
    }
  )
);
