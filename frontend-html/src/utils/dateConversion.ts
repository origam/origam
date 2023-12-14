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

// https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
// https://momentjs.com/docs/#/displaying/format/

export function csToMomentFormat(csDateFormat: string | undefined) {
  if (!csDateFormat || !isValidCsFormat(csDateFormat)) {
    return null;
  }
  return csDateFormat                       // Meaning of the replaced character in c#:
    // The day of the month, from 1 through 31., do not replace "ddd" and "dddd" those are valid day of week symbols
    // Implemented by a replacement fn. due to Safari lack of negative lookbehind support.
    .replace(/d+/g, (capturedString) => 
      capturedString.length <= 2 
        ? capturedString.replace(/d/g,'D') 
        : capturedString
      ) 
    .replace(/f/g, "S") // The tenths of a second in a date and time value.
    .replace(/F/g, "S") // If non-zero, the tenths of a second in a date and time value.
    .replace(/K/g, "Z") // time zone
    .replace(/tt/g, "A") // AM/PM,
    .replace(/y/g, "Y") // year,
    .replace(/g/g, "N") // The period or era. A.D. / B.C.
    ;
}

function isValidCsFormat(candidate: string) {
  return candidate.match(/^[,\s.\-:/yMdHmsfFghKst]*$/g) !== null;
  //there is no direct equivalent to "z" in moment
}