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

export function firstGteIndex(
  elementGetter: (index: number) => number,
  elementCount: number,
  num: number
) {
  let indexLeft = 0;
  let indexRight = elementCount - 1;
  const T = num;
  let element;
  while (indexLeft <= indexRight) {
    const currentIndex = Math.floor((indexLeft + indexRight) / 2);
    element = elementGetter(currentIndex);
    if (element < T) {
      indexLeft = currentIndex + 1;
    } else if (element > T) {
      indexRight = currentIndex - 1;
    } else {
      return currentIndex;
    }
  }
  return Math.max(indexLeft, indexRight);
}

export function lastLteIndex(
  elementGetter: (index: number) => number,
  elementCount: number,
  num: number
) {
  let indexLeft = 0;
  let indexRight = elementCount - 1;
  const T = num;
  let element;
  while (indexLeft <= indexRight) {
    const currentIndex = Math.floor((indexLeft + indexRight) / 2);
    element = elementGetter(currentIndex);
    if (element < T) {
      indexLeft = currentIndex + 1;
    } else if (element > T) {
      indexRight = currentIndex - 1;
    } else {
      return currentIndex;
    }
  }
  return Math.min(indexLeft, indexRight);
}

export function rangeQuery(
  elementGetterL: (index: number) => number,
  elementGetterR: (index: number) => number,
  elementCount: number,
  start: number,
  end: number
) {
  const firstGreaterThanNumber = firstGteIndex(elementGetterL, elementCount, start);
  const lastLessThanNumber = lastLteIndex(elementGetterR, elementCount, end);
  return {lastLessThanNumber, firstGreaterThanNumber};
}