/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import React, { useContext, useEffect, useMemo, useRef, useState } from "react";

import CS from "gui/Components/ScreenElements/Editors/CommonStyle.module.css";
import S from "gui/Components/ScreenElements/Editors/TagInputEditor.module.css";

import { TagInput, TagInputAdd, TagInputItem, TagInputItemDelete, } from "gui/Components/TagInput/TagInput";
import { inject, observer } from "mobx-react";
import { IProperty } from "model/entities/types/IProperty";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { CtxDropdownEditor } from "modules/Editors/DropdownEditor/DropdownEditor";
import { requestFocus } from "utils/focus";
import { CtxDropdownRefCtrl } from "gui/Components/Dropdown/DropdownCommon";

export const TagInputEditor = inject(({property}: { property: IProperty }, {value}) => {
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
      backgroundColor?: string;
      foregroundColor?: string;
      customStyle?: any;
      onChange?(event: any, value: string[]): void;
      onKeyDown?(event: any): void;
      onClick?(event: any): void;
      onDoubleClick?(event: any): void;
      onEditorBlur?(event: any): void;
      customInputClass?: string;
      autoFocus?: boolean;
      id?: string;
    }) => {
      const beh = useContext(CtxDropdownEditor).behavior;
      const ref = useContext(CtxDropdownRefCtrl);
      const data = useContext(CtxDropdownEditor).editorData;

      const value = Array.isArray(props.value) ? [...props.value] : props.value;
      data.setValue(value);

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

      const [inputElement, setInputElement] = useState<any | undefined>(undefined);

      const refInput = useMemo(() => {
        return (elm: any) => {
          beh.refInputElement(elm);
          setInputElement(elm);
        };
      }, [beh]);

      useEffect(() => {
        if (beh.subscribeToFocusManager && beh.elmInputElement) {
          beh.subscribeToFocusManager(beh.elmInputElement);
        }
        if (props.autoFocus) {
          setTimeout(() => requestFocus(inputElement));
        }
      }, [beh, inputElement, props.autoFocus]);

      const previousValueRef = useRef<string[]>();

      useEffect(() => {
        if (previousValueRef.current?.length !== value?.length) {
          beh.elmInputElement.value = "";
        }
        previousValueRef.current = value;
      }, [ // eslint-disable-line react-hooks/exhaustive-deps
        previousValueRef.current,
        previousValueRef.current?.length, // eslint-disable-line react-hooks/exhaustive-deps
        value,
        value?.length, // eslint-disable-line react-hooks/exhaustive-deps
        beh.elmInputElement?.value, // eslint-disable-line react-hooks/exhaustive-deps
      ]);

      useEffect(() => {
        beh.mount();
        return () => {
         props.onEditorBlur?.({target: beh.elmInputElement});
        }
      }, []);

      async function handleInputKeyDown(event: any) {
        if (event.key === "Backspace" && event.target.value === "" && value.length > 0) {
          removeItem(event, value[value.length - 1]);
        }
        await beh.handleInputKeyDown(event);
        props.onKeyDown?.(event);
      }

      return (
        <div className={CS.editorContainer} ref={ref}>
          <TagInput className={"tagInput"}>
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
              <TagInputAdd onClick={(event) => {
                beh.handleInputBtnClick(event);
                requestFocus(beh.elmInputElement);
              }}/>
            )}
            <input
              id={props.id}
              disabled={props.isReadOnly}
              className={S.filterInput + " " + props.customInputClass}
              ref={refInput}
              placeholder={data.isResolving ? "Loading..." : ""}
              onChange={beh.handleInputChange}
              onKeyDown={handleInputKeyDown}
              onFocus={beh.handleInputFocus}
              onBlur={beh.handleInputBlur}
              onDoubleClick={props.onDoubleClick}
              autoComplete={"off"}
              style={getStyle()}
              size={1}
            />
          </TagInput>
        </div>
      );
    }
  )
);
