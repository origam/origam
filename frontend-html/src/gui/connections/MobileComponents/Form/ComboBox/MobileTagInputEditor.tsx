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


import CS from "gui/Components/ScreenElements/Editors/CommonStyle.module.css";
import S from "gui/Components/ScreenElements/Editors/TagInputEditor.module.css";

import { TagInput, TagInputAdd, TagInputItem, TagInputItemDelete, } from "gui/Components/TagInput/TagInput";
import { observer } from "mobx-react";
import { IProperty } from "model/entities/types/IProperty";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import React from "react";

export const MobileTagInputEditor = (
  observer(
    (props: {
      isReadOnly: boolean;
      backgroundColor?: string;
      foregroundColor?: string;
      customStyle?: any;
      customInputClass?: string;
      id?: string;
      property: IProperty;
      onPlusButtonClick: () => void;
      onChange?: (event: any, newValue: string[]) => void;
      values?: any[];
    }) => {
      const dataTable = getDataTable(props.property);

      const values = props.values ?? getTagInputValues(props.property);
      const textValues = values.map((valueItem: any) => dataTable.resolveCellText(props.property, valueItem));

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
        const index = values.indexOf(item);
        if (index > -1) {
          values.remove(item);
          props.onChange?.(event, values);
        }
      }

      function onPlusButtonClick() {
        if (props.isReadOnly) {
          return;
        }
        props.onPlusButtonClick();
      }

      return (
        <div className={CS.editorContainer + " tagEditorContainer"}>
          <TagInput className={S.tagInput}>
            {values.map((valueItem, idx) => (
              <TagInputItem key={valueItem}>
                {!props.isReadOnly &&
                  <TagInputItemDelete
                    onClick={(event) => {
                      removeItem(event, valueItem);
                    }}
                  />}
                {textValues![idx] || ""}
              </TagInputItem>
            ))}
            {props.isReadOnly ? null : (
              <TagInputAdd onClick={onPlusButtonClick}/>
            )}
            <input
              id={props.id}
              disabled={props.isReadOnly}
              className={S.filterInput + " " + props.customInputClass}
              autoComplete={"new-password"}
              style={getStyle()}
              size={1}
            />
          </TagInput>
        </div>
      );
    }
  )
);

export function getTagInputValues(property: IProperty): any[] {
  const dataTable = getDataTable(property);
  const row = getSelectedRow(property);
  if (!row) {
    return [];
  }

  const cellValue = dataTable.getCellValue(row, property) ?? [];
  return (Array.isArray(cellValue) ? [...cellValue] : cellValue) as any[];
}
