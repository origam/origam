import {IColumnSettings} from "./types/IColumnSettings";

export function compareByGroupingIndex( settings1: IColumnSettings, settings2: IColumnSettings ) {
  if(!settings1.groupingIndex || !settings2.groupingIndex){
    throw new Error("groupingIndices must be defined in order to compare them")
  }
  if ( settings1.groupingIndex < settings2.groupingIndex ){
    return -1;
  }
  if ( settings1.groupingIndex > settings2.groupingIndex ){
    return 1;
  }
  return 0;
}