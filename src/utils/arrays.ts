export function firstGteIndex(
  elementGetter: (index: number) => number,
  elementCount: number,
  num: number
) {
  let indexLeft= 0;
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
  return { lastLessThanNumber, firstGreaterThanNumber };
}

declare global {
  interface Array<T> {
    remove(o: T): Array<T>;
  }
}

declare global {
  interface Array<T> {
    groupBy<T, K>(keyGetter: (key: T) => K):  Map<K, T[]>;
  }
}

Array.prototype.remove = function(item){
  const index = this.indexOf(item);
  if (index > -1) {
    this.splice(index, 1);
  }
  return this;
}

Array.prototype.groupBy = function<T, K>(keyGetter: (key: T) => K) {
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