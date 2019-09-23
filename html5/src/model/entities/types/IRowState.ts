import { any } from "prop-types";

export interface IRowStateData {}

export interface IRowState extends IRowStateData {
  $type_IRowState: 1;

  getValue(key: string): IRowStateItem | undefined;

  parent?: any;
}

export interface IRowStateItem {
  id: string;
  allowCreate: boolean;
  allowDelete: boolean;
  foregroundColor: string;
  backgroundColor: string;
  columns: Map<string, IRowStateColumnItem>;
  disabledActions: Set<string>;
}

export interface IRowStateColumnItem {
  name: string;
  foregroundColor: string;
  backgroundColor: string;
  allowRead: boolean;
  allowUpdate: boolean;
}


export interface IIdState {}
