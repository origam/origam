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
import { useContext, useEffect, useState } from "react";
import { action, computed, observable } from "mobx";
import { getDefaultCsDateFormatDataFromCookie } from "utils/cookies";
import DateCompleter from "gui/Components/ScreenElements/Editors/DateCompleter";
import moment, { Moment } from "moment";
import SD from "gui/Components/ScreenElements/Editors/DateTimeEditor/DateTimeEditor.module.scss";
import S from "gui/connections/MobileComponents/Form/MobileDateTimeEditor.module.scss"
import cx from "classnames";
import { EditLayoutState } from "model/entities/MobileState/MobileLayoutState";
import { MobileState } from "model/entities/MobileState/MobileState";
import { toOrigamServerString } from "@origam/utils";
import { IProperty } from "model/entities/types/IProperty";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { CalendarWidget } from "gui/Components/ScreenElements/Editors/DateTimeEditor/CalendarWidget";


class DateEditorState {
  constructor(
    value: string | null,
    public outputFormat: string,
    private onChange?: (event: any, isoDay: string | undefined | null) => void,
    private onClick?: (event: any) => void,
    private onKeyDown?: (event: any) => void,
    private onEditorBlur?: (event: any) => void,
    private onChangeByCalendar?: (event: any, isoDay: string) => void
  ) {
    this.value = value;
  }

  @observable
  value: string | null;

  // @observable isDroppedDown = false;

  @observable isShowFormatHintTooltip = false;

  // refDropdowner = (elm: Dropdowner | null) => (this.elmDropdowner = elm);
  // elmDropdowner: Dropdowner | null = null;


  // disposers: any[] = [];
  //
  // componentWillUnmount() {
  //   this.disposers.forEach((d) => d());
  // }

  // componentDidUpdate(prevProps: { value: string | null }) {
  //   runInAction(() => {
  //     if (prevProps.value !== null && this.value === null) {
  //       this.dirtyTextualValue = "";
  //     }
  //   });
  // }

  // @action.bound
  // makeFocusedIfNeeded() {
  //   setTimeout(() => {
  //     if ((this.props.autoFocus) && this.elmInput) {
  //       this.elmInput.select();
  //       this.elmInput.focus();
  //       this.elmInput.scrollLeft = 0;
  //     }
  //   });
  // }

  _hLocationInterval: any;

  // @action.bound
  // handleWindowMouseWheel(e: any) {
  //   this.setShowFormatHint(false);
  // }

  // @action.bound
  // setShowFormatHint(state: boolean) {
  //   if (state && !this.isShowFormatHintTooltip) {
  //     this.measureInputElement();
  //     this._hLocationInterval = setInterval(() => this.measureInputElement(), 500);
  //     window.addEventListener("mousewheel", this.handleWindowMouseWheel);
  //   } else if (!state && this.isShowFormatHintTooltip) {
  //     clearInterval(this._hLocationInterval);
  //     window.removeEventListener("mousewheel", this.handleWindowMouseWheel);
  //   }
  //   this.isShowFormatHintTooltip = state;
  // }

  @action.bound handleInputBlur(event: any) {
    // this.setShowFormatHint(false);
    const dateCompleter = this.getDateCompleter();
    const completedMoment = dateCompleter.autoComplete(this.dirtyTextualValue);
    if (completedMoment) {
      this.onChange?.(event, toOrigamServerString(completedMoment));
    } else if (this.momentValue?.isValid()) {
      this.onChange?.(event, toOrigamServerString(this.momentValue));
    }

    this.dirtyTextualValue = undefined;
    this.onEditorBlur && this.onEditorBlur(event);
  }

  private get autoCompletedMoment() {
    const dateCompleter = this.getDateCompleter();
    return dateCompleter.autoComplete(this.dirtyTextualValue);
  }

  get autocompletedText() {
    const completedMoment = this.autoCompletedMoment;
    if (completedMoment) {
      if (this.autoCompletedMoment?.isValid()) {
        return this.formatMomentValue(this.autoCompletedMoment);
      } else return "?";
    } else {
      if (this.momentValue?.isValid()) {
        return this.formatMomentValue(this.momentValue);
      } else return "?";
    }
  }

  // @action.bound handleKeyDown(event: any) {
  //   if (event.key === "Enter" || event.key === "Tab") {
  //     const completedMoment = this.autoCompletedMoment;
  //     if (completedMoment) {
  //       this.props.onChange?.(event, toOrigamServerString(completedMoment));
  //     } else if (this.momentValue?.isValid()) {
  //       this.props.onChange?.(event, toOrigamServerString(this.momentValue));
  //     }
  //     this.dirtyTextualValue = undefined;
  //   } else if (event.key === "Escape") {
  //     this.setShowFormatHint(false);
  //   }
  //   this.props.onKeyDown?.(event);
  // }

  @action.bound getDateCompleter() {
    const formatData = getDefaultCsDateFormatDataFromCookie();
    return new DateCompleter(
      formatData.defaultDateSequence,
      this.outputFormat,
      formatData.defaultDateSeparator,
      formatData.defaultTimeSeparator,
      formatData.defaultDateTimeSeparator,
      () => moment()
    );
  }


  @action.bound handleTextFieldChange(event: any) {
    // this.setShowFormatHint(true);
    this.dirtyTextualValue = event.target.value;
    if (this.dirtyTextualValue === "") {
      this.onChange && this.onChange(event, null);
      return;
    }
  }

  @action.bound handleKeyDown(event: any) {
    if (event.key === "Enter" || event.key === "Tab") {
      const completedMoment = this.autoCompletedMoment;
      if (completedMoment) {
        this.onChange?.(event, toOrigamServerString(completedMoment));
      } else if (this.momentValue?.isValid()) {
        this.onChange?.(event, toOrigamServerString(this.momentValue));
      }
      this.dirtyTextualValue = undefined;
    } else if (event.key === "Escape") {
      // this.setShowFormatHint(false);
    }
    this.onKeyDown?.(event);
  }

  // @action.bound handleContainerMouseDown(event: any) {
  //   setTimeout(() => {
  //     this.elmInput?.focus();
  //   }, 30);
  // }

  // refContainer = (elm: HTMLDivElement | null) => (this.elmContainer = elm);
  // elmContainer: HTMLDivElement | null = null;
  // refInput = (elm: HTMLInputElement | null) => {
  //   this.elmInput = elm;
  // };
  // @observable inputRect: any;
  // elmInput: HTMLInputElement | null = null;

  // @action.bound
  // measureInputElement() {
  //   if (this.elmInput) {
  //     this.inputRect = this.elmInput.getBoundingClientRect();
  //   } else {
  //     this.inputRect = undefined;
  //   }
  // }

  @observable dirtyTextualValue: string | undefined;

  get momentValue() {
    if (this.dirtyTextualValue) {
      return moment(this.dirtyTextualValue, this.outputFormat);
    }
    return !!this.value ? moment(this.value) : null;
  }

  formatMomentValue(value: Moment | null | undefined) {
    if (!value) return "";
    return value.format(this.outputFormat);
  }

  get formattedMomentValue() {
    return this.formatMomentValue(this.momentValue);
  }

  @computed get textFieldValue() {
    return this.dirtyTextualValue !== undefined && this.dirtyTextualValue !== ""
      ? this.dirtyTextualValue
      : this.formattedMomentValue;
  }

  @computed get isTooltipShown() {
    return (
      this.textFieldValue !== undefined &&
      (!moment(this.textFieldValue, this.outputFormat) ||
        this.formattedMomentValue !== this.textFieldValue)
    );
  }

  @action.bound handleDayClick(event: any, day: moment.Moment) {
    // this.elmDropdowner && this.elmDropdowner.setDropped(false);
    this.dirtyTextualValue = undefined;
    this.onChangeByCalendar && this.onChangeByCalendar(event, day.toISOString(true));
    this.onChange && this.onChange(event, toOrigamServerString(day));
  }
}

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
    new DateEditorState(
      props.value,
      props.outputFormat,
      props.onChange,
      props.onClick,
      props.onKeyDown,
      props.onEditorBlur,
      props.onChangeByCalendar)
  );

  useEffect(() => {
    setEditorState(
      new DateEditorState(
        props.value,
        props.outputFormat,
        props.onChange,
        props.onClick,
        props.onKeyDown,
        props.onEditorBlur,
        props.onChangeByCalendar));
  }, [props.value]);

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
  editorState: DateEditorState;
  property: IProperty
}> = observer((props) => {

  const dataTable = getDataTable(props.property);
  let row = getSelectedRow(props.property)!;
  props.editorState.value = dataTable.getCellValue(row, props.property);

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
