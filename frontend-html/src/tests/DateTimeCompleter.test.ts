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
import DateCompleter from "../gui/Components/ScreenElements/Editors/DateCompleter";
import { DateSequence } from "utils/cookies";

function format(dateTime: moment.Moment, expectedFormat: string): string {
  if (dateTime.hour() === 0 && dateTime.minute() === 0 && dateTime.second() === 0) {
    const expectedDateFormat = expectedFormat.split(" ")[0]
    return dateTime.format(expectedDateFormat)
  }
  return dateTime.format(expectedFormat)
}

const dateCompleterUs = new DateCompleter(DateSequence.MonthDayYear, "/",
  ":", " ", () => moment("3/15/2020", "M/D/YYYY"))

test.each([
  ["5 ", "3/5/2020"],
  ["129", "12/9/2020"],
  ["12092018", "12/9/2018"],
  ["120915", "12/9/2015"],
  ["129 1230", "12/9/2020 12:30:00 PM"],
  ["129 12", "12/9/2020 12:00:00 PM"],
  ["1209 12", "12/9/2020 12:00:00 PM"],
  ["12092020 123020", "12/9/2020 12:30:20 PM"],
  ["6/5/2020", "6/5/2020"],
  ["6/5", "6/5/2020"],
  ["6/5 14", "6/5/2020 2:00:00 PM"],
  ["6/5 1430", "6/5/2020 2:30:00 PM"],
  ["6/5 14:30", "6/5/2020 2:30:00 PM"],
  ["6/5/16", "6/5/2016"],
])('Should auto complete %s to: %s using dateCompleterUs', (incompleteDate, expected) => {
  const momentValue = dateCompleterUs.autoComplete(incompleteDate)
  expect(format(momentValue!, "M/D/YYYY h:mm:ss A")).toBe(expected);
});


const dateCompleterCz = new DateCompleter(DateSequence.DayMonthYear, ".",
  ":", " ", () => moment("12/15/2017", "M/D/YYYY"))

test.each([
  ["0912", "09.12.2017"],
  ["0912", "09.12.2017"],
  ["09122020", "09.12.2020"],
  ["091220", "09.12.2020"],
  ["0912 1230", "09.12.2017 12:30:00"],
  ["0912 12", "09.12.2017 12:00:00"],
  ["09122020 123020", "09.12.2020 12:30:20"],
  ["5 ", "05.12.2017"],
  ["05 ", "05.12.2017"],
  ["5.6", "05.06.2017"],
  ["5.6.", "05.06.2017"],
  ["5.6.2018", "05.06.2018"],
  ["5.6 14", "05.06.2017 14:00:00"],
  ["5.6 1430", "05.06.2017 14:30:00"],
  ["5.6 14:30", "05.06.2017 14:30:00"],
  ["5.6.16", "05.06.2016"],
])('Should auto complete %s to: %s using dateCompleterCz', (incompleteDate, expected) => {
  const momentValue = dateCompleterCz.autoComplete(incompleteDate)
  expect(format(momentValue!, "DD.MM.YYYY HH:mm:ss")).toBe(expected);
});

test.each([
  ["12:30"],
  ["12:30:14"],
  ["12:30:14.16"],
])('Should not try to complete time alone using dateCompleterCz', (incompleteDate) => {
  const momentValue = dateCompleterCz.autoComplete(incompleteDate)
  expect(momentValue).toBe(undefined);
});

test.each([
  ["12:30"],
  ["12:30:14"],
  ["12:30:14.16"],
])('Should not try to complete time alone using dateCompleterUs', (incompleteDate) => {
  const momentValue = dateCompleterUs.autoComplete(incompleteDate)
  expect(momentValue).toBe(undefined);
});

test.each([
  ["13. září 2020 10:00"],
])('Should not try to complete time when input is in incompatible format using dateCompleterCz', (incompleteDate) => {
  const momentValue = dateCompleterCz.autoComplete(incompleteDate)
  expect(momentValue).toBe(undefined);
});

test.each([
  ["13. september 2020 10:00"],
])('Should not try to complete time when input is in incompatible format using dateCompleterUs', (incompleteDate) => {
  const momentValue = dateCompleterUs.autoComplete(incompleteDate)
  expect(momentValue).toBe(undefined);
});