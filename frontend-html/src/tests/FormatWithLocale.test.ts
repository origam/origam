import {formatNumberWithLocale} from "../model/entities/NumberFormating";


test.each([
  ["#.000", 123456.7890123, "de-CH", "123456.789"],
  ["#.000", 123456.7, "de-CH", "123456.700"],
  ["#.###", 123456.7890123, "de-CH", "123456.789"],
  ["#.###", 123456.7, "de-CH", "123456.7"],
  ["### ###.###", 123456.7, "de-CH", "123,456.7"],
  ["######.###", 123456.7, "de-CH", "123456.7"],
  ["######.00#", 123456.7, "de-CH", "123456.70"],
])('Format %s to: %s', (customNumericFormat: string, value: number, locale: string, expected: string ) => {
  const formattedValue = formatNumberWithLocale(customNumericFormat, value, locale)
  expect(formattedValue).toBe(expected);
});
