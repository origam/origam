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
declare global {
  interface Array<T> {
    remove(o: T): Array<T>;
  }
}

declare global {
  interface Array<T> {
    average(): T;
  }
}

declare global {
  interface Array<T> {
    sum(): T;
  }
}

declare global {
  interface Array<T> {
    groupBy<K>(keyGetter: (key: T) => K): Map<K, T[]>;
  }
}

Array.prototype.remove = function (item) {
  const index = this.indexOf(item);
  if (index > -1) {
    this.splice(index, 1);
  }
  return this;
}

Array.prototype.average = function () {
  return this.reduce((a, b) => a + b) / this.length;
}

Array.prototype.sum = function () {
  return this.reduce((a, b) => a + b, 0);
}

Array.prototype.groupBy = function <T, K>(keyGetter: (key: T) => K) {
  const map = new Map<K, T[]>();
  this.forEach((item) => {
    const key = keyGetter(item);
    const collection = map.get(key);
    if (!collection) {
      map.set(key, [item]);
    } else {
      collection.push(item);
    }
  });
  return map;
}