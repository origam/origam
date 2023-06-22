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

import { formatNumberWithLocale } from "../model/entities/NumberFormating";


test.each([
  ["#.000", 123456.7890123, "de-CH", "123456.789"],
  ["#.000", 123456.7, "de-CH", "123456.700"],
  ["#.###", 123456.7890123, "de-CH", "123456.789"],
  ["#.###", 123456.7, "de-CH", "123456.7"],
  ["### ###.###", 123456.7, "de-CH", "123’456.7"],
  ["######.###", 123456.7, "de-CH", "123456.7"],
  ["######.00#", 123456.7, "de-CH", "123456.70"],
])('Format %s to: %s', (customNumericFormat: string, value: number, locale: string, expected: string) => {
  const formattedValue = formatNumberWithLocale(customNumericFormat, value, locale)
  expect(formattedValue).toBe(expected);
});
