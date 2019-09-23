import { any } from "prop-types";

export interface IRowStateData {}

export interface IRowState extends IRowStateData {
  $type_IRowState: 1;

  getValue(key: string): IRowStateItem;

  parent?: any;
}

export type IRowStateItem = any;

export interface IIdState {}
