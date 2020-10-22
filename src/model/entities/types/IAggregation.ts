import {AggregationType, parseAggregationType} from "./AggregationType";
import {IAggregationInfo} from "model/entities/types/IAggregationInfo";

export interface IAggregation {
  columnId: string;
  type: AggregationType
  value: number;
}
