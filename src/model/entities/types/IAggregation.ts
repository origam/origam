import {AggregationType, parseAggregationType} from "./AggregationType";

export interface IAggregation {
  columnId: string;
  type: AggregationType
  value: number;
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