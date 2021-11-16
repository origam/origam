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

import cx from "classnames";
import { Dropdowner } from "gui/Components/Dropdowner/Dropdowner";
import { action, computed, observable, runInAction } from "mobx";
import { observer, Observer } from "mobx-react";
import moment, { Moment } from "moment";
import * as React from "react";
import { toOrigamServerString } from "utils/moment";
import { IFocusable } from "../../../../model/entities/FormFocusManager";
import { getDefaultCsDateFormatDataFromCookie } from "../../../../utils/cookies";
import DateCompleter from "./DateCompleter";
import S from "./DateTimeEditor.module.scss";
import { createPortal } from "react-dom";

@observer
class CalendarWidget extends React.Component<{
  initialDisplayDate?: moment.Moment;
  selectedDay?: moment.Moment;
  onDayClick?(event: any, day: moment.Moment): void;
}> {
  @observable.ref displayedMonth = moment(this.props.initialDisplayDate).startOf("month");

  getDayHeaders() {
    const result: any[] = [];
    for (
      let day = moment(this.displayedMonth).startOf("week"), i = 0;
      i < 7;
      day.add({days: 1}), i++
    ) {
      result.push(
        <div
          key={day.toISOString()}
          className={S.calendarWidgetDayHeaderCell}
          title={day.format("dddd")}
        >
          {day.format("dd")[0]}
        </div>
      );
    }
    return result;
  }

  getDates(weekInc: number) {
    const result: any[] = [];
    const today = moment();
    for (
      let day = moment(this.displayedMonth).startOf("week").add({weeks: weekInc}), i = 0;
      i < 7;
      day.add({days: 1}), i++
    ) {
      const isNeighbourMonth = day.month() !== this.displayedMonth.month();
      const isSelectedDay = day.isSame(this.props.selectedDay, "day");
      const isToday = day.isSame(today, "days");
      const dayCopy = moment(day);
      result.push(
        <div
          key={day.toISOString()}
          className={cx(S.calendarWidgetCell, {
            [S.calendarWidgetNeighbourMonthCell]: isNeighbourMonth,
            [S.calendarWidgetSelectedDay]: isSelectedDay,
            isToday,
          })}
          onClick={(event: any) => this.handleDayClick(event, dayCopy)}
        >
          {day.format("D")}
        </div>
      );
    }
    return result;
  }

  getDateRows() {
    const result: any[] = [];
    for (let i = 0; i < 6; i++) {
      result.push(<div key={i} className={S.calendarWidgetRow}>{this.getDates(i)}</div>);
    }
    return result;
  }

  @action.bound
  handleMonthDecClick(event: any) {
    this.displayedMonth = moment(this.displayedMonth).subtract({months: 1});
  }

  @action.bound
  handleMonthIncClick(event: any) {
    this.displayedMonth = moment(this.displayedMonth).add({months: 1});
  }

  @action.bound
  handleYearDecClick(event: any) {
    this.displayedMonth = moment(this.displayedMonth).subtract({years: 1});
  }

  @action.bound
  handleYearIncClick(event: any) {
    this.displayedMonth = moment(this.displayedMonth).add({years: 1});
  }

  @action.bound handleDayClick(event: any, day: moment.Moment) {
    this.props.onDayClick && this.props.onDayClick(event, day);
  }

  render() {
    return (
      <div
        className={S.calendarWidgetContainer}
        onMouseDown={(e) => {
          /* Prevent mousedown default action causong onblur being fired on the editors input
              which triggered data update
          */
          e.preventDefault();
        }}
      >
        <div className={S.calendarWidgetDayTable}>
          <div className={S.calendarWidgetRow}>
            <div className={S.calendarWidgetHeader}>
              <div className={S.calendarWidgetMonthControls}>
                <button className={S.calendarWidgetControlBtn} onClick={this.handleMonthDecClick}>
                  <i className="fas fa-caret-left"/>
                </button>
                <button className={S.calendarWidgetControlBtn} onClick={this.handleMonthIncClick}>
                  <i className="fas fa-caret-right"/>
                </button>
              </div>
              <div
                className={S.calendarWidgetTitle}
                title={this.displayedMonth.format("YYYY MMMM")}
              >
                {this.displayedMonth.format("YYYY MMMM")}
              </div>
              <div className={S.calendarWidgetYearControls}>
                <button className={S.calendarWidgetControlBtn} onClick={this.handleYearDecClick}>
                  <i className="fas fa-caret-down"/>
                </button>
                <button className={S.calendarWidgetControlBtn} onClick={this.handleYearIncClick}>
                  <i className="fas fa-caret-up"/>
                </button>
              </div>
            </div>
          </div>
          <div className={S.calendarWidgetRow}>{this.getDayHeaders()}</div>
          {this.getDateRows()}
        </div>
      </div>
    );
  }
}

@observer
export class DateTimeEditor extends React.Component<{
  id?: string;
  value: string | null;
  outputFormat: string;
  outputFormatToShow: string;
  isReadOnly?: boolean;
  isInvalid?: boolean;
  invalidMessage?: string;
  autoFocus?: boolean;
  foregroundColor?: string;
  backgroundColor?: string;
  onChange?: (event: any, isoDay: string | undefined | null) => void;
  onChangeByCalendar?: (event: any, isoDay: string) => void;
  onClick?: (event: any) => void;
  onDoubleClick?: (event: any) => void;
  onKeyDown?: (event: any) => void;
  onEditorBlur?: (event: any) => void;
  refocuser?: (cb: () => void) => () => void;
  subscribeToFocusManager?: (obj: IFocusable) => void;
}> {
  @observable isDroppedDown = false;

  @observable isShowFormatHintTooltip = false;

  refDropdowner = (elm: Dropdowner | null) => (this.elmDropdowner = elm);
  elmDropdowner: Dropdowner | null = null;

  @action.bound handleDropperClick(event: any) {
    event.stopPropagation();
    this.makeDroppedDown();
  }

  @action.bound makeDroppedDown() {
    if (!this.isDroppedDown) {
      this.isDroppedDown = true;
      window.addEventListener("click", this.handleWindowClick);
    }
  }

  @action.bound makeDroppedUp() {
    if (this.isDroppedDown) {
      this.isDroppedDown = false;
      window.removeEventListener("click", this.handleWindowClick);
    }
  }

  @action.bound handleWindowClick(event: any) {
    if (this.elmContainer && !this.elmContainer.contains(event.target)) {
      this.makeDroppedUp();
    }
  }

  disposers: any[] = [];

  componentDidMount() {
    this.makeFocusedIfNeeded();
    if (this.elmInput && this.props.subscribeToFocusManager) {
      this.props.subscribeToFocusManager(this.elmInput);
    }
  }

  componentWillUnmount() {
    this.disposers.forEach((d) => d());
  }

  componentDidUpdate(prevProps: { value: string | null }) {
    runInAction(() => {
      if (prevProps.value !== null && this.props.value === null) {
        this.dirtyTextualValue = "";
      }
    });
  }

  @action.bound
  makeFocusedIfNeeded() {
    setTimeout(() => {
      if ((this.props.autoFocus) && this.elmInput) {
        this.elmInput.select();
        this.elmInput.focus();
        this.elmInput.scrollLeft = 0;
      }
    });
  }

  _hLocationInterval: any;

  @action.bound
  handleWindowMouseWheel(e: any) {
    this.setShowFormatHint(false);
  }

  @action.bound
  setShowFormatHint(state: boolean) {
    if (state && !this.isShowFormatHintTooltip) {
      this.measureInputElement();
      this._hLocationInterval = setInterval(() => this.measureInputElement(), 500);
      window.addEventListener("mousewheel", this.handleWindowMouseWheel);
    } else if (!state && this.isShowFormatHintTooltip) {
      clearInterval(this._hLocationInterval);
      window.removeEventListener("mousewheel", this.handleWindowMouseWheel);
    }
    this.isShowFormatHintTooltip = state;
  }

  @action.bound handleInputBlur(event: any) {
    this.setShowFormatHint(false);
    const dateCompleter = this.getDateCompleter();
    const completedMoment = dateCompleter.autoComplete(this.dirtyTextualValue);
    if (completedMoment) {
      this.props.onChange?.(event, toOrigamServerString(completedMoment));
    } else if (this.momentValue?.isValid()) {
      this.props.onChange?.(event, toOrigamServerString(this.momentValue));
    }

    this.dirtyTextualValue = undefined;
    this.props.onEditorBlur && this.props.onEditorBlur(event);
  }

  private get autoCompletedMoment() {
    const dateCompleter = this.getDateCompleter();
    return dateCompleter.autoComplete(this.dirtyTextualValue);
  }

  private get autocompletedText() {
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

  @action.bound handleKeyDown(event: any) {
    if (event.key === "Enter" || event.key === "Tab") {
      const completedMoment = this.autoCompletedMoment;
      if (completedMoment) {
        this.props.onChange?.(event, toOrigamServerString(completedMoment));
      } else if (this.momentValue?.isValid()) {
        this.props.onChange?.(event, toOrigamServerString(this.momentValue));
      }
      this.dirtyTextualValue = undefined;
    } else if (event.key === "Escape") {
      this.setShowFormatHint(false);
    }
    this.props.onKeyDown?.(event);
  }

  @action.bound getDateCompleter() {
    const formatData = getDefaultCsDateFormatDataFromCookie();
    return new DateCompleter(
      formatData.defaultDateSequence,
      this.props.outputFormat,
      formatData.defaultDateSeparator,
      formatData.defaultTimeSeparator,
      formatData.defaultDateTimeSeparator,
      () => moment()
    );
  }

  @action.bound handleContainerMouseDown(event: any) {
    setTimeout(() => {
      this.elmInput?.focus();
    }, 30);
  }

  refContainer = (elm: HTMLDivElement | null) => (this.elmContainer = elm);
  elmContainer: HTMLDivElement | null = null;
  refInput = (elm: HTMLInputElement | null) => {
    this.elmInput = elm;
  };
  @observable inputRect: any;
  elmInput: HTMLInputElement | null = null;

  @action.bound
  measureInputElement() {
    if (this.elmInput) {
      this.inputRect = this.elmInput.getBoundingClientRect();
    } else {
      this.inputRect = undefined;
    }
  }

  @observable dirtyTextualValue: string | undefined;

  get momentValue() {
    if (this.dirtyTextualValue) {
      return moment(this.dirtyTextualValue, this.props.outputFormat);
    }
    return !!this.props.value ? moment(this.props.value) : null;
  }

  formatMomentValue(value: Moment | null | undefined) {
    if (!value) return "";
    // if (value.hour() === 0 && value.minute() === 0 && value.second() === 0) {
    //   const expectedDateFormat = this.props.outputFormat.split(" ")[0];
    //   return value.format(expectedDateFormat);
    // }
    return value.format(this.props.outputFormat);
  }

  get formattedMomentValue() {
    return this.formatMomentValue(this.momentValue);
  }

  @computed get textfieldValue() {
    return this.dirtyTextualValue !== undefined && this.dirtyTextualValue !== ""
      ? this.dirtyTextualValue
      : this.formattedMomentValue;
  }

  @computed get isTooltipShown() {
    return (
      this.textfieldValue !== undefined &&
      (!moment(this.textfieldValue, this.props.outputFormat) ||
        this.formattedMomentValue !== this.textfieldValue)
    );
  }

  @action.bound handleTextfieldChange(event: any) {
    this.setShowFormatHint(true);
    this.dirtyTextualValue = event.target.value;
    if (this.dirtyTextualValue === "") {
      this.props.onChange && this.props.onChange(event, null);
      return;
    }
  }

  @action.bound handleDayClick(event: any, day: moment.Moment) {
    this.elmDropdowner && this.elmDropdowner.setDropped(false);
    this.dirtyTextualValue = undefined;
    this.props.onChangeByCalendar && this.props.onChangeByCalendar(event, day.toISOString(true));
    this.props.onChange && this.props.onChange(event, toOrigamServerString(day));
  }

  @action.bound
  handleFocus(event: any) {
    if (this.elmInput) {
      this.elmInput.select();
      this.elmInput.scrollLeft = 0;
    }
  }

  customFormatContainsDate() {
    return (
      this.props.outputFormat.includes("D") ||
      this.props.outputFormat.includes("M") ||
      this.props.outputFormat.includes("Y")
    );
  }

  renderWithCalendarWidget() {
    return (
      <Dropdowner
        ref={this.refDropdowner}
        onContainerMouseDown={this.handleContainerMouseDown}
        trigger={({refTrigger, setDropped}) => (
          <div
            className={S.editorContainer}
            ref={this.refContainer}
            style={{
              zIndex: this.isDroppedDown ? 1000 : undefined,
            }}
          >
            <Observer>
              {() => (
                <>
                  {this.isShowFormatHintTooltip && (
                    <FormatHintTooltip
                      boundingRect={this.inputRect}
                      line1={this.autocompletedText}
                      line2={this.props.outputFormatToShow}
                    />
                  )}
                  <input
                    id={this.props.id}
                    title={this.autocompletedText + '\n' + this.props.outputFormatToShow}
                    style={{
                      color: this.props.foregroundColor,
                      backgroundColor: this.props.backgroundColor,
                    }}
                    className={S.input}
                    type="text"
                    onBlur={this.handleInputBlur}
                    onFocus={this.handleFocus}
                    ref={(elm) => {
                      this.refInput(elm);
                    }}
                    value={this.textfieldValue}
                    readOnly={this.props.isReadOnly}
                    onChange={this.handleTextfieldChange}
                    onClick={this.props.onClick}
                    onDoubleClick={this.props.onDoubleClick}
                    onKeyDown={this.handleKeyDown}
                  />
                </>
              )}
            </Observer>

            {this.props.isInvalid && (
              <div className={S.notification} title={this.props.invalidMessage}>
                <i className="fas fa-exclamation-circle red"/>
              </div>
            )}
            {!this.props.isReadOnly && (
              <div
                className={S.dropdownSymbol}
                onMouseDown={() => setDropped(true)}
                ref={refTrigger}
              >
                <i className="far fa-calendar-alt"/>
              </div>
            )}
          </div>
        )}
        content={() => (
          <div className={S.droppedPanelContainer}>
            <CalendarWidget
              onDayClick={this.handleDayClick}
              initialDisplayDate={this.momentValue?.isValid() ? this.momentValue : moment()}
              selectedDay={this.momentValue?.isValid() ? this.momentValue : moment()}
            />
          </div>
        )}
      />
    );
  }

  renderInputFieldOnly() {
    return (
      <div
        className={S.editorContainer}
        ref={this.refContainer}
        style={{
          zIndex: this.isDroppedDown ? 1000 : undefined,
        }}
      >
        <input
          id={this.props.id}
          style={{
            color: this.props.foregroundColor,
            backgroundColor: this.props.backgroundColor,
          }}
          title={this.autocompletedText + '\n' + this.props.outputFormat}
          className={S.input}
          type="text"
          onBlur={this.handleInputBlur}
          onFocus={this.handleFocus}
          ref={this.refInput}
          value={this.textfieldValue}
          readOnly={this.props.isReadOnly}
          onChange={this.handleTextfieldChange}
          onClick={this.props.onClick}
          onDoubleClick={this.props.onDoubleClick}
          onKeyDown={this.handleKeyDown}
        />
        {this.props.isInvalid && (
          <div className={S.notification} title={this.props.invalidMessage}>
            <i className="fas fa-exclamation-circle red"/>
          </div>
        )}
      </div>
    );
  }

  render() {
    if (!this.props.outputFormat || this.customFormatContainsDate()) {
      return this.renderWithCalendarWidget();
    } else {
      return this.renderInputFieldOnly();
    }
  }
}

function FormatHintTooltip(props: { boundingRect?: any; line1?: string; line2?: string }) {
  const [tooltipHeight, setTooltipHeight] = React.useState(0);

  function refTooltip(elm: any) {
    if (elm) {
      setTooltipHeight(elm.getBoundingClientRect().height);
    }
  }

  if (!props.boundingRect || (!props.line1 && !props.line2)) return null;
  const bounds = props.boundingRect;

  return createPortal(
    <div
      ref={refTooltip}
      className={S.formatHintTooltip}
      style={{top: bounds.top - 1 - tooltipHeight, left: bounds.left}}
    >
      {props.line1}
      {props.line2 && <br/>}
      {props.line2}
    </div>,
    document.getElementById("tooltip-portal")!
  );
}
