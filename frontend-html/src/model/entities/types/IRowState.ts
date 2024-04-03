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

export interface IRowStateData {
}

export interface IRowState extends IRowStateData {
  $type_IRowState: 1;

  mayCauseFlicker: boolean;

  isWorking: boolean;

  getValue(key: string): IRowStateItem | undefined;

  loadValues(keys: string[]): Promise<any>;

  clearValue(rowId: string): void;

  putValue(state: any): void;

  hasValue(key: string): boolean;

  suppressWorkingStatus: boolean;

  clearAll(): void;
  reload(): void;

  parent?: any;

  dispose(): void;
}

export interface IRowStateItem {
  id: string;
  allowCreate: boolean;
  allowDelete: boolean;
  foregroundColor: string | undefined;
  backgroundColor: string | undefined;
  columns: Map<string, IRowStateColumnItem>;
  disabledActions: Set<string>;
  relations: any[];
}

export interface IRowStateColumnItem {
  name: string;
  dynamicLabel: string | null | undefined;
  foregroundColor: string | undefined;
  backgroundColor: string | undefined;
  allowRead: boolean;
  allowUpdate: boolean;
}

export interface IIdState {
}
