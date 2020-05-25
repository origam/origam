
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
  getOrdering(column: string): IOrderByColumnSetting;
  setOrdering(column: string): void;
  addOrdering(column: string): void;
  groupChildrenOrdering: IGroupChildrenOrdering | undefined;
  getDefaultOrdering(): IOrdering | undefined;
  parent?: any;
}

export interface IOrdering {
  columnId: string;
  direction: IOrderByDirection;
}

export interface IGroupChildrenOrdering {
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
export function parseToOrdering(candidate: any): IOrdering | undefined{
  if(!candidate || !candidate.field || !candidate.direction) return undefined;

  return {
    columnId: candidate.field,
    direction: parseToIOrderByDirection(candidate.direction),
  };
}
