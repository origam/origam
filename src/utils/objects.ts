export function map2obj(map: Map<string, any>) {
  const result: { [key: string]: any } = {};
  for (let [k, v] of map.entries()) {
    result[k] = v;
  }
  return result;
}

function splitPropertyPair(pair: string){
  return pair.split(":").map(x => x.trim())
}

export function cssString2Object(cssString: string | undefined){
  if(!cssString){
    return undefined;
  }
  return cssString
    .split(";")
    .filter(pair=>pair !== "")
    .map(pair => splitPropertyPair(pair))
    .reduce((obj: any, pair)=> {
        obj[pair[0]] = pair[1];
        return obj;
      }
      ,{})
}

