import DateCompleter from "../DateCompleter"
import moment from "moment";

function format(dateTime: moment.Moment, expectedFormat: string): string {
  if (dateTime.hour() == 0 && dateTime.minute() == 0 && dateTime.second() == 0) {
    const expectedDateFormat = expectedFormat.split(" ")[0]
    return dateTime.format(expectedDateFormat)
  }
  return dateTime.format(expectedFormat)
}

const dateCompleterUs = new DateCompleter("M/D/YYYY h:mm:ss A", "/",
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
  ["5/5", "5/5/2020"],
  ["5/5 14", "5/5/2020 2:00:00 PM"],
  ["5/5 1430", "5/5/2020 2:30:00 PM"],
  ["5/5 14:30", "5/5/2020 2:30:00 PM"],
  ["5/5/16", "5/5/2016"],
])('Should auto complete %s to: %s', (incompleteDate, expected) => {
  const momentValue = dateCompleterUs.autoComplete(incompleteDate)
  expect(format(momentValue, "M/D/YYYY h:mm:ss A")).toBe(expected);
});


const dateCompleterCz = new DateCompleter("DD.MM.YYYY h:mm:ss", ".",
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
])('Should auto complete %s to: %s', (incompleteDate, expected) => {
  const momentValue = dateCompleterCz.autoComplete(incompleteDate)
  expect(format(momentValue, "DD.MM.YYYY h:mm:ss")).toBe(expected);
});