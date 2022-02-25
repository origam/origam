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

import { IColumnConfiguration } from "model/entities/TablePanelView/types/IConfigurationManager";
import { observable } from "mobx";
import { AggregationType } from "model/entities/types/AggregationType";
import { GroupingUnit } from "model/entities/types/GroupingUnit";

export class TableColumnConfiguration implements IColumnConfiguration {

  @observable
  aggregationType: AggregationType | undefined;
  @observable
  groupingIndex: number = 0;
  @observable
  isVisible: boolean = true;
  @observable
  timeGroupingUnit: GroupingUnit | undefined;
  @observable
  width;

  constructor(
    public propertyId: string,
    defaultWidth: number
  ) {
    this.width = defaultWidth;
  }

  deepClone(): IColumnConfiguration {
    return Object.assign(Object.create(Object.getPrototypeOf(this)), this)
  }
}