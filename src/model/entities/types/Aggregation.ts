import {IAggregation} from "../../../gui/Components/ScreenElements/Table/TableRendering/types";

export interface Aggregation {
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

export function parseAggregations(objectArray: any[] | undefined): IAggregation[] | undefined{
  if(!objectArray) return undefined;
  return objectArray.map(object =>
  {
    return {
      columnId: object["column"],
      type: parseAggregationType(object["type"]),
      value: object["value"]
    }
  });
}

export function aggregationToString(aggregation: IAggregation){
  function round(value: number){
    return Math.round(value * 100)/100
  }
 if(aggregation.type === AggregationType.SUM){
  return "Σ " + round(aggregation.value)
 }
  return aggregation.type + ": " + round(aggregation.value)
}


