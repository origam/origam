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
import { MobXProviderContext, observer } from "mobx-react";
import { IProperty } from "model/entities/types/IProperty";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import React, { useContext } from "react";
import { onFieldChange } from "model/actions-ui/DataView/TableView/onFieldChange";
import { ComboEditLayoutState } from "model/entities/MobileState/MobileLayoutState";
import { MobileState } from "model/entities/MobileState/MobileState";
import { XmlBuildDropdownEditor } from "gui/connections/MobileComponents/Form/ComboBox/ComboBox";
import { IDataView } from "model/entities/types/IDataView";

export const TagInputEditor = (
  observer(
    (props: {
      xmlNode: any;
      isReadOnly: boolean;
      backgroundColor?: string;
      foregroundColor?: string;
      customStyle?: any;
      customInputClass?: string;
      autoFocus?: boolean;
      id?: string;
      property: IProperty;
      dataView: IDataView;
    }) => {

      const mobileState = useContext(MobXProviderContext).application.mobileState as MobileState;

      const dataTable = getDataTable(props.property);
      const row = getSelectedRow(props.property);
      if(row === undefined){
        return <div/>;
      }

      const cellValue = dataTable.getCellValue(row, props.property);
      const value = (Array.isArray(cellValue) ? [...cellValue] : cellValue) as any[];
      const textValues = value?.map((valueItem: any) => dataTable.resolveCellText(props.property, valueItem));

      function onChange(event: any, newValue: string[]){
        onFieldChange(props.property)({
          event: event,
          row: row!,
          property: props.property,
          value: newValue,
        });
      }

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
          value.remove(item);
          onChange(event, value);
        }
      }

      function onPlusButtonClick(){
        if(props.isReadOnly){
          return;
        }
        mobileState.layoutState = new ComboEditLayoutState(
          <XmlBuildDropdownEditor
            {...props}
            editingTags={true}
          />)
      }

      return (
        <div className={CS.editorContainer}>
          <TagInput className={S.tagInput}>
            {value.map((valueItem, idx) => (
                <TagInputItem key={valueItem}>
                  <TagInputItemDelete
                    onClick={(event) => {
                      removeItem(event, valueItem);
                    }}
                  />
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
