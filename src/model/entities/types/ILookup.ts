import { IDropDownColumn } from "./IDropDownColumn";
import { CancellablePromise } from "mobx/lib/api/flow";

export enum IDropDownType {
  EagerlyLoadedGrid = "EagerlyLoadedGrid",
  LazilyLoadedGrid = "LazilyLoadedGrid",
  EagerlyLoadedTree = "EagerlyLoadedTree",
}

export interface IDropDownParameter {
  parameterName: string;
  fieldName: string;
}

export interface ILookupData {
  lookupId: string;
  dropDownShowUniqueValues: boolean;
  identifier: string;
  identifierIndex: number;
  dropDownType: IDropDownType;
  cached: boolean;
  searchByFirstColumnOnly: boolean;
  dropDownColumns: IDropDownColumn[];
  dropDownParameters: IDropDownParameter[];
}

export interface ILookup extends ILookupData {
  $type_ILookup: 1;

  // getValue(key: string): any;
  isLoading(key: string): boolean;
  resolveList(ids: Set<string>): CancellablePromise<any>;

  parameters: { [key: string]: any };
  parent?: any;
}

export const isILookup = (o: any): o is ILookup => o.$type_ILookup;
