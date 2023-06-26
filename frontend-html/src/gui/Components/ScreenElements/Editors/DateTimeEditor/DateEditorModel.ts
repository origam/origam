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


import { action, computed, flow, observable } from "mobx";
import { toOrigamServerString } from "@origam/utils";
import { getDefaultCsDateFormatDataFromCookie } from "utils/cookies";
import DateCompleter from "gui/Components/ScreenElements/Editors/DateCompleter";
import moment, { Moment } from "moment";

export interface IEditorState{
  value: string | null;
}

export class DateEditorModel {
  constructor(
    private editorState: IEditorState,
    public outputFormat: string,
    private onChange?: (event: any, isoDay: string | undefined | null) => Promise<void>,
    private onClick?: (event: any) => void,
    private onKeyDown?: (event: any) => void,
    private onEditorBlur?: (event: any) => Promise<void>,
    private onChangeByCalendar?: (event: any, isoDay: string) => void
  ) {
  }

  @action.bound
  async handleInputBlur(event: any) {
    const self = this;
    await flow(function*() {
      const dateCompleter = self.getDateCompleter();
      const completedMoment = dateCompleter.autoComplete(self.dirtyTextualValue);
      if (completedMoment) {
        yield self.onChange?.(event, toOrigamServerString(completedMoment));
      } else if (self.momentValue?.isValid()) {
        yield self.onChange?.(event, toOrigamServerString(self.momentValue));
      }

      self.dirtyTextualValue = undefined;
      if(self.onEditorBlur){
        yield self.onEditorBlur(event);
      }
    })();
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
    }
    else if (event.key === " " && !this.dirtyTextualValue) {
      const timeNow = moment();
      const dayFormatOnly =
        !this.outputFormat.includes("H") &&
        !this.outputFormat.includes("m") &&
        !this.outputFormat.includes("s");
      if(dayFormatOnly){
        timeNow.set({hour:0, minute:0, second:0, millisecond:0})
      }
      this.onChange?.(event, toOrigamServerString(timeNow));
    }
    else if (event.key === "Escape") {
    }
    this.onKeyDown?.(event);
  }

  @observable dirtyTextualValue: string | undefined;

  get momentValue() {
    if (this.dirtyTextualValue) {
      return moment(this.dirtyTextualValue, this.outputFormat);
    }
    return !!this.editorState.value ? moment(this.editorState.value) : null;
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


  @action.bound handleDayClick(event: any, day: moment.Moment) {
    this.dirtyTextualValue = undefined;
    this.onChangeByCalendar && this.onChangeByCalendar(event, day.toISOString(true));
    this.onChange && this.onChange(event, toOrigamServerString(day));
  }

  onClearClick(event: any){
    this.dirtyTextualValue = undefined;
    this.onChange && this.onChange(event, null);
  }
}
