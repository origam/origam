export function map2obj(map: Map<string, any>) {
  const result: { [key: string]: any } = {};
  for (let [k, v] of map.entries()) {
    result[k] = v;
  }
  return result;
}
