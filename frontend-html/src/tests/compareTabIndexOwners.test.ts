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

import {compareTabIndexOwners, TabIndex} from "model/entities/TabIndexOwner";


test.each([
  [["1", "5"], -1],
  [["5", "5"], 0],
  [["10", "5"], 1],
  [["1.12 ", "1.15"], -1],
  [["1.12", "1.2"], 1],
  [["1.12", "1.12"], 0],
  [["5", "5.2"], -1],
  [["5.12.14", "5.12.5"], 1],
  [["5.12.5", "5.12.15"], -1],
  [["5.12.5", "5.12.5"], 0],
  [["5", "5.12.5"], -1],
  [["1000000", "1"], 1],
  [[undefined, undefined], 0],
  [[undefined, "1"], 1],
  [["1", undefined], -1],
])('Compare %s to: %s', (values, expectedNormalizedResult) => {
  const val1 = {tabIndex: TabIndex.create(values[0])}
  const val2 = {tabIndex: TabIndex.create(values[1])}
  const comparisonResult = compareTabIndexOwners(val1, val2)

  const sign = Math.sign(comparisonResult);
  const normalizedResult = sign !== 0
    ? comparisonResult / comparisonResult * sign
    : comparisonResult;

  expect(normalizedResult).toBe(expectedNormalizedResult);
});