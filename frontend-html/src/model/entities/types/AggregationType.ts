/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

export enum AggregationType { SUM = "SUM", AVG = "AVG", MIN = "MIN", MAX = "MAX", COUNT = "COUNT"}

export function tryParseAggregationType(candidate: any | undefined){
  if(!candidate || candidate === "0") return undefined;
  return parseAggregationType(candidate);
}


export function aggregationTypeToNumber(aggregationType: AggregationType | undefined){
  switch (aggregationType) {
    case undefined:           return 0;
    case AggregationType.SUM: return 1;
    case AggregationType.AVG: return 2;
    case AggregationType.MIN: return 3;
    case AggregationType.MAX: return 4;
    case AggregationType.COUNT: return 5;
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
    case "5":
    case "COUNT": return AggregationType.COUNT;
    default: throw new Error("Cannot map \""+candidate+"\" to AggregationType")
  }
}
