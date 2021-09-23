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

import { Moment } from "moment";
import { GroupingUnit } from "./types/GroupingUnit";
import { getLocaleFromCookie } from "utils/cookies";


export interface IGroupData{
  value: any;
  label: string;
  rows: any[][];
  compare(other: IGroupData): number;
}

export class GenericGroupData implements IGroupData{
  private readonly _label: any;

  constructor(public value: string, label: any)
  {
    this._label = Array.isArray(label) ? label.join(", "): label;
  }

  public get label(){
    return this._label;
  }
  public rows: any[][] = [];

  compare(other: IGroupData): number{
    if (this.label && other.label) {
      if(typeof this.label === "string"){
        return this.label.localeCompare(other.label, getLocaleFromCookie());
      }else{
        return this.label > other.label ? 1 : -1;
      }
    } else if (!this.label) {
      return -1;
    } else {
      return 1;
    }
  }
}

export class DateGroupData implements IGroupData {
  constructor(
    public value: Moment | undefined,
    public label: string
  ) {
  }

  public rows: any[][] = [];

  public static create(value: Moment, groupingUnit: GroupingUnit): IGroupData {
    if (!value.isValid()) {
      new DateGroupData(undefined, "");
    }

    let groupLabel = "";
    switch (groupingUnit) {
      case GroupingUnit.Year:
        value.set({ 'month': 0, 'date': 1, 'hour': 0, 'minute': 0, 'second': 0 });
        groupLabel = value.format("YYYY");
        break;
      case GroupingUnit.Month:
        value.set({ 'date': 1, 'hour': 0, 'minute': 0, 'second': 0 });
        groupLabel = value.format("YYYY-MM");
        break;
      case GroupingUnit.Day:
        value.set({ 'hour': 0, 'minute': 0, 'second': 0 });
        groupLabel = value.format("YYYY-MM-DD");
        break;
      case GroupingUnit.Hour:
        value.set({ 'minute': 0, 'second': 0 });
        groupLabel = value.format("YYYY-MM-DD h:00");
        break;
      case GroupingUnit.Minute:
        value.set({ 'second': 0 });
        groupLabel = value.format("YYYY-MM-DD h:mm");
        break;
    }
    return new DateGroupData(value, groupLabel);
  }

  compare(other: IGroupData): number {
    if (this.value && !other.value) return -1;
    if (!this.value && other.value) return 1;
    if (!this.value && !other.value) return 0;
    if (this.value!.isValid() && other.value.isValid()) {
      if (this.value! > other.value) {
        return 1;
      }
      else if (this.value! < other.value) {
        return -1;
      }
      else {
        return 0;
      }
    } else if (!this.value) {
      return -1;
    } else {
      return 1;
    }
  }
}
