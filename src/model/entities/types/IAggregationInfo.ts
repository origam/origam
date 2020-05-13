export interface IAggregationInfo {
  ColumnName: string;
  AggregationType: AggregationType;
}

export enum AggregationType{ SUM="SUM", AVG="AVG", MIN="MIN", MAX="MAX"}

export function tryParseAggregationType(candidate: any | undefined){
  if(!candidate) return undefined;
  return parseAggregationType(candidate);
}

export function parseAggregationType(candidate: any | undefined){
  if(typeof candidate !== 'string'){
    throw new Error("Cannot map \""+candidate+"\" to AggregationType")
  }

  switch ((candidate as string).toUpperCase()) {
    case "SUM": return AggregationType.SUM;
    case "AVG": return AggregationType.AVG;
    case "MIN": return AggregationType.MIN;
    case "MAX": return AggregationType.MAX;
    default: throw new Error("Cannot map \""+candidate+"\" to AggregationType")
  }
}
