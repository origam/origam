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


import { MobXProviderContext, observer } from "mobx-react";
import * as React from "react";
import { useContext, useState } from "react";
import moment from "moment";
import SD from "gui/Components/ScreenElements/Editors/DateTimeEditor/DateTimeEditor.module.scss";
import S from "gui/connections/MobileComponents/Form/MobileDateTimeEditor.module.scss"
import cx from "classnames";
import { EditLayoutState } from "model/entities/MobileState/MobileLayoutState";
import { MobileState } from "model/entities/MobileState/MobileState";
import { IProperty } from "model/entities/types/IProperty";
import { CalendarWidget } from "gui/Components/ScreenElements/Editors/DateTimeEditor/CalendarWidget";
import {
  DateEditorModel,
  IEditorState,
} from "gui/Components/ScreenElements/Editors/DateTimeEditor/DateEditorModel";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";


export const MobileDateTimeEditor: React.FC<{
  id?: string;
  value: string | null;
  outputFormat: string;
  outputFormatToShow: string;
  isReadOnly?: boolean;
  autoFocus?: boolean;
  foregroundColor?: string;
  backgroundColor?: string;
  onChange: (event: any, isoDay: string | undefined | null) => void;
  onChangeByCalendar?: (event: any, isoDay: string) => void;
  onClick?: (event: any) => void;
  onDoubleClick?: (event: any) => void;
  onKeyDown?: (event: any) => void;
  onEditorBlur: (event: any) => void;
  property: IProperty
}> = observer((props) => {

  const mobileState = useContext(MobXProviderContext).application.mobileState as MobileState;

  const [editorState, setEditorState] = useState(
    new DateEditorModel(
      new MobileEditorState(props.property),
      props.outputFormat,
      props.onChange,
      props.onClick,
      props.onKeyDown,
      props.onEditorBlur,
      props.onChangeByCalendar)
  );

  function onClick() {
    mobileState.layoutState = new EditLayoutState(
      <FullScreenDateTimeEditor
        id={props.id}
        editorState={editorState}
        onClick={props.onClick}
        property={props.property}
      />
    )
  }

  return (
    <div className={SD.editorContainer}>
      <div
        id={props.id}
        style={{
          color: props.foregroundColor,
          backgroundColor: props.backgroundColor,
        }}
        className={cx(S.input, SD.input)}
      >
        {editorState.textFieldValue}
      </div>

      {!props.isReadOnly && (
        <div
          onClick={onClick}
          className={SD.dropdownSymbol}
        >
          <i className="far fa-calendar-alt"/>
        </div>
      )}
    </div>
  );
});

export const FullScreenDateTimeEditor: React.FC<{
  id?: string;
  onClick?: (event: any) => void;
  editorState: DateEditorModel;
  property: IProperty
}> = observer((props) => {

  return (
    <div className={S.fullScreenEditorRoot}>
      <div className={S.fullScreenEditorInputContainer}>
        <div className={S.inputRow}>
          <input
            id={props.id}
            className={cx("input", S.input, S.fullScreenEditorInput)}
            type="text"
            onBlur={props.editorState.handleInputBlur}
            value={props.editorState.textFieldValue}
            onChange={props.editorState.handleTextFieldChange}
            onClick={props.onClick}
            onKeyDown={props.editorState.handleKeyDown}
          />
          <button
            className={S.autoCompleteButton}
          >
            Auto complete
          </button>
        </div>
        <div className={S.fullScreenEditorInfoRow}>
          {"Format: " + props.editorState.outputFormat}
        </div>
        <div className={S.fullScreenEditorInfoRow}>
          {"Preview: " + props.editorState.autocompletedText}
        </div>
      </div>
      <CalendarWidget
        onDayClick={props.editorState.handleDayClick}
        initialDisplayDate={props.editorState.momentValue?.isValid() ? props.editorState.momentValue : moment()}
        selectedDay={props.editorState.momentValue?.isValid() ? props.editorState.momentValue : moment()}
      />
    </div>
  );
});

export class MobileEditorState implements IEditorState{

  constructor(private property: IProperty) {
  }

  get value(){
    const dataTable = getDataTable(this.property);
    let row = getSelectedRow(this.property);
    if(!row){
      return null;
    }
    return dataTable.getCellValue(row, this.property);
  }
}
