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


export enum IOrderByDirection {
  NONE = "NONE",
  ASC = "ASC",
  DESC = "DESC"
}


export interface IOrderByColumnSetting {
  ordering: IOrderByDirection;
  order: number;
}

export interface IOrderingConfiguration {
  userOrderings: IOrdering[];
  orderings: IOrdering[];
  getOrdering(column: string): IOrderByColumnSetting;
  setOrdering(column: string): void;
  addOrdering(column: string): void;
  groupChildrenOrdering: IOrdering | undefined;
  getDefaultOrderings(): IOrdering[];
  parent?: any;
  orderingFunction: () => (row1: any[], row2: any[]) => number;
}

export interface IOrdering {
  columnId: string;
  direction: IOrderByDirection;
  lookupId: string | undefined;
}

export function parseToIOrderByDirection(candidate: string | undefined): IOrderByDirection{
  switch(candidate){
    case "Descending": return IOrderByDirection.DESC;
    case "Ascending": return IOrderByDirection.ASC;
    case undefined: return IOrderByDirection.NONE;
    default: throw new Error("Option not implemented: " + candidate)
  }
}
export function parseToOrdering(candidateArray: any[]): IOrdering[] | undefined{
  if(!candidateArray || candidateArray.length === 0) return undefined;

  return candidateArray.filter(candidate => candidate.field || candidate.direction)
    .map(candidate => {
      return {
        columnId: candidate.field,
        direction: parseToIOrderByDirection(candidate.direction),
        lookupId: undefined
      };
  })
}
