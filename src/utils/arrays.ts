export function firstGTE(
  elementGetter: (index: number) => number,
  n: number,
  num: number
) {
  let L = 0;
  let R = n - 1;
  const T = num;
  let element;
  while (L <= R) {
    const m = Math.floor((L + R) / 2);
    element = elementGetter(m);
    if (element < T) {
      L = m + 1;
    } else if (element > T) {
      R = m - 1;
    } else {
      return m;
    }
  }
  return Math.max(L, R);
}

export function lastLTE(
  elementGetter: (index: number) => number,
  n: number,
  num: number
) {
  let L = 0;
  let R = n - 1;
  const T = num;
  let element;
  while (L <= R) {
    const m = Math.floor((L + R) / 2);
    element = elementGetter(m);
    if (element < T) {
      L = m + 1;
    } else if (element > T) {
      R = m - 1;
    } else {
      return m;
    }
  }
  return Math.min(L, R);
}

export function rangeQuery(
  elementGetterL: (index: number) => number,
  elementGetterR: (index: number) => number,
  n: number,
  start: number,
  end: number
) {
  const firstGreaterThanNumber = firstGTE(elementGetterL, n, start);
  const lastLessThanNumber = lastLTE(elementGetterR, n, end);
  return { lastLessThanNumber, firstGreaterThanNumber };
}
