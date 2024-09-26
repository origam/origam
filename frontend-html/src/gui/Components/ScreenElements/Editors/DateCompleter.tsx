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

import moment from "moment";
import { DateSequence } from "utils/cookies";

export default class DateCompleter {
  dateSequence: DateSequence;
  dateSeparator: string;
  timeSeparator: string;
  dateTimeSeparator: string;
  timeNowFunc: () => moment.Moment;

  constructor(
    dateSequence: DateSequence,
    dateSeparator: string,
    timeSeparator: string,
    dateTimeSeparator: string,
    timeNowFunc: () => moment.Moment
  ) {
    this.dateSeparator = dateSeparator;
    this.timeSeparator = timeSeparator;
    this.dateTimeSeparator = dateTimeSeparator;
    this.timeNowFunc = timeNowFunc;
    this.dateSequence = dateSequence;
  }

  isUSCompletionTriggered(datetimeTextIn: string) {
    const t = datetimeTextIn;
    return (
      t.match(/^\d\d?\d?$/) ||
      t.match(/^\d\d\d\d\d\d\d\d$/) ||
      t.match(/^\d\d\d\d\d\d$/) ||
      t.match(/^\d\d\d\s+\d\d\d\d$/) ||
      t.match(/^\d\d\d\d?\s+\d\d$/) ||
      t.match(/^\d\d\d\d\d\d\d\d\s+\d\d\d\d\d\d$/) ||
      t.match(new RegExp("^\\d\\d?\\" + this.dateSeparator + "\\d?\\d?\\" + this.dateSeparator + "?\\d?\\d?\\d?\\d?$")) ||
      t.match(new RegExp("^\\d\\d?\\" + this.dateSeparator + "\\d?\\d?\\" + this.dateSeparator + "?\\d?\\d?\\d?\\d? +\\d?\\d?:?\\d?\\d?:?\\d?\\d?$"))
    );
  }

  isEUCompletionTriggered(datetimeTextIn: string) {
    const t = datetimeTextIn;
    return (
      t.match(/^\d\d\d\d$/) ||
      t.match(/^\d\d\d\d\d\d\d\d$/) ||
      t.match(/^\d\d\d\d\d\d$/) ||
      t.match(/^\d\d\d\d\s+\d\d\d\d$/) ||
      t.match(/^\d\d\d\d\s+\d\d$/) ||
      t.match(/^\d\d\d\d\d\d\d\d \d\d\d\d\d\d$/) ||
      t.match(/^\d\d?$/) ||
      t.match(new RegExp("^\\d\\d?\\" + this.dateSeparator + "\\d?\\d?\\" + this.dateSeparator + "?\\d?\\d?\\d?\\d?$")) ||
      t.match(new RegExp("^\\d\\d?\\" + this.dateSeparator + "\\d?\\d?\\" + this.dateSeparator + "?\\d?\\d?\\d?\\d? +\\d?\\d?:?\\d?\\d?:?\\d?\\d?$"))
    );
  }

  autoComplete(text: string | undefined): moment.Moment | undefined {
    if (!text) {
      return undefined;
    }
    const trimmedText = text.trim();
    if (this.dateSequence === DateSequence.MonthDayYear) {
      if (!this.isUSCompletionTriggered(trimmedText)) return;
    } else {
      if (!this.isEUCompletionTriggered(trimmedText)) return;
    }
    const dateAndTime = trimmedText.split(this.dateTimeSeparator);
    const dateText = dateAndTime[0];
    let completeDate = this.autoCompleteDate(dateText);
    let parsingFormat = this.parsingDateFormat();
    if (dateAndTime.length === 2) {
      const timeText = dateAndTime[1];
      const completeTime = this.autoCompleteTime(timeText);
      completeDate += this.dateTimeSeparator + completeTime;
      parsingFormat = this.parsingDateTimeFormat();
    }
    return moment(completeDate, parsingFormat);
  }

  autoCompleteTime(incompleteTime: string): string {
    if (incompleteTime.includes(this.timeSeparator)) {
      return this.completeTimeWithSeparators(incompleteTime);
    }
    return this.completeTimeWithoutSeparators(incompleteTime);
  }

  completeTimeWithoutSeparators(incompleteTime: string): string {
    switch (incompleteTime.length) {
      case 1:
      case 2:
        return incompleteTime + this.timeSeparator + "00" + this.timeSeparator + "00";
      case 3:
      case 4:
        return (
          incompleteTime.substring(0, 2) +
          this.timeSeparator +
          incompleteTime.substring(2) +
          this.timeSeparator +
          "00"
        );
      default:
        return (
          incompleteTime.substring(0, 2) +
          this.timeSeparator +
          incompleteTime.substring(2, 4) +
          this.timeSeparator +
          incompleteTime.substring(4)
        );
    }
  }

  completeTimeWithSeparators(incompleteTime: string): string {
    const splitTime = incompleteTime.split(this.timeSeparator);
    if (splitTime.length === 2) {
      return moment([2010, 1, 1, splitTime[0], splitTime[1], 0, 0]).format(this.parsingTimeFormat());
    }
    if (splitTime.length === 3) {
      return moment([2010, 1, 1, splitTime[0], splitTime[1], splitTime[2], 0]).format(this.parsingTimeFormat());
    }
    return incompleteTime;
  }

  autoCompleteDate(incompleteDate: string): string {
    return incompleteDate.includes(this.dateSeparator)
      ? this.completeDateWithSeparators(incompleteDate)
      : this.completeDateWithoutSeparators(incompleteDate);
  }

  completeDateWithSeparators(incompleteDate: string): string {
    const splitDate = incompleteDate
      .split(this.dateSeparator)
      .filter(x => x !== "");
    if (splitDate.length === 2) {
      return incompleteDate + this.dateSeparator + this.timeNowFunc().year();
    } else {
      return incompleteDate;
    }
  }

  completeDateWithoutSeparators(incompleteDate: string): string {
    switch (incompleteDate.length) {
      case 1:
      case 2: {
        // assuming input is always a day.
        return this.addMonthAndYear(incompleteDate);
      }
      case 3:
      case 4: {
        // assuming input is day and month in order specified by
        // current culture
        return this.addYear(incompleteDate);
      }
      case 6: {
        // assuming input is day and month in order specified by
        // current culture followed by incomplete year (yy)
        return this.addSeparators(incompleteDate);
      }
      default:
        return this.addSeparators(incompleteDate);
    }
  }

  addMonthAndYear(day: string): string {
    const now = this.timeNowFunc();
    const usDateString = now.month() + 1 + "/" + day + "/" + now.year();

    const date = moment(usDateString, "M/D/YYYY");

    return date ? date.format(this.parsingDateFormat()) : day;
  }

  addYear(dayAndMonth: string): string {
    return (
      dayAndMonth.substring(0, 2) +
      this.dateSeparator +
      dayAndMonth.substring(2) +
      this.dateSeparator +
      this.timeNowFunc().year()
    );
  }

  addSeparators(incompleteDate: string): string {
    const format = this.parsingDateTimeFormat();

    const firstIndex = format.indexOf(this.dateSeparator);
    const secondIndex = format.lastIndexOf(this.dateSeparator);
    const dateLength = incompleteDate.length;

    if (firstIndex < dateLength && secondIndex >= dateLength) {
      return (
        incompleteDate.substring(0, firstIndex) +
        this.dateSeparator +
        incompleteDate.substring(firstIndex)
      );
    }
    if (firstIndex < dateLength && secondIndex < dateLength) {
      return (
        incompleteDate.substring(0, firstIndex) +
        this.dateSeparator +
        incompleteDate.substring(firstIndex, firstIndex + 2) +
        this.dateSeparator +
        incompleteDate.substring(secondIndex - 1)
      );
    }
    return incompleteDate;
  }

  parsingDateTimeFormat(): string {
    return `${this.parsingDateFormat()} ${this.parsingTimeFormat()}`;
  }

  parsingTimeFormat(){
    return `HH${this.timeSeparator}mm${this.timeSeparator}ss`;
  }

  private parsingDateFormat() {
    switch (this.dateSequence) {
      case DateSequence.DayMonthYear:
        return `DD${this.dateSeparator}MM${this.dateSeparator}YYYY`;
      case DateSequence.MonthDayYear:
        return `MM${this.dateSeparator}DD${this.dateSeparator}YYYY`;
      default:
        throw new Error(`${this.dateSequence} not implemented`);
    }
  }
}
