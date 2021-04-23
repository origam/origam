
// https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
// https://momentjs.com/docs/#/displaying/format/
export function csToMomentFormat(csDateFormat: string){
  if(!isValidCsFormat(csDateFormat)){
    return null;
  }
  return csDateFormat                       // Meaning of the replaced character in c#:
    .replace(/d/g, "D") // The day of the month, from 1 through 31.,
    .replace(/f/g, "S") // The tenths of a second in a date and time value.
    .replace(/F/g, "S") // If non-zero, the tenths of a second in a date and time value.
    .replace(/K/g, "Z") // time zone
    .replace(/t/g, "A") // AM/PM,
    .replace(/y/g, "Y") // year,
    .replace(/g/g, "N"); // The period or era. A.D. / B.C.
}

function isValidCsFormat(candidate: string){
  return candidate.match(/^[\s.\-:/yMdHmsfFghKst]*$/g) !== null;
  //there is no direct equivalent to "z" in moment
}