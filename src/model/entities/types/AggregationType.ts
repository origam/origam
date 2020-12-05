export enum AggregationType { SUM = "SUM", AVG = "AVG", MIN = "MIN", MAX = "MAX"}

export function tryParseAggregationType(candidate: any | undefined){
  if(!candidate) return undefined;
  return parseAggregationType(candidate);
}


export function aggregationTypeToNumber(aggregationType: AggregationType | undefined){
  switch (aggregationType) {
    case undefined:           return 0;
    case AggregationType.SUM: return 1;
    case AggregationType.AVG: return 2;
    case AggregationType.MIN: return 3;
    case AggregationType.MAX: return 4;
    default: throw new Error("Cannot map \""+aggregationType+"\" to number")
  }
}

export function parseAggregationType(candidate: any | undefined){
  if(typeof candidate !== 'string'){
    throw new Error("Cannot map \""+candidate+"\" to AggregationType")
  }

  switch ((candidate as string).toUpperCase()) {
    case "1":
    case "SUM": return AggregationType.SUM;
    case "2":
    case "AVG": return AggregationType.AVG;
    case "3":
    case "MIN": return AggregationType.MIN;
    case "4":
    case "MAX": return AggregationType.MAX;
    default: throw new Error("Cannot map \""+candidate+"\" to AggregationType")
  }
}
