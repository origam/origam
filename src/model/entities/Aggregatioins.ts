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

import {AggregationType, parseAggregationType} from "model/entities/types/AggregationType";
import {IDataView} from "model/entities/types/IDataView";
import {IAggregationInfo} from "model/entities/types/IAggregationInfo";
import {getProperties} from "model/selectors/DataView/getProperties";
import {IAggregation} from "model/entities/types/IAggregation";
import { IProperty } from "./types/IProperty";
import { formatNumber } from "./NumberFormating";

export function parseAggregations(objectArray: any[] | undefined): IAggregation[] | undefined{
  if(!objectArray) return undefined;
  return objectArray.map(object =>
  {
    return {
      columnId: object["column"],
      type: parseAggregationType(object["type"]),
      value: object["value"]
    }
  });
}

export function aggregationToString(aggregation: IAggregation, property: IProperty){
  function round(value: number){
    return Math.round(value * 100)/100
  }
  const formattedValue = formatNumber(
    property.customNumericFormat,
    property.entity ?? '',
    round(aggregation.value));
  
  if(aggregation.type === AggregationType.SUM){
    return "Σ " + formattedValue
  }
  return aggregation.type + ": " + formattedValue
}

export function calcAggregations(dataView: IDataView, aggregationInfos: IAggregationInfo[]): IAggregation[] {
  return aggregationInfos.map(aggregationInfo => {
    return {
      columnId: aggregationInfo.ColumnName,
      type: aggregationInfo.AggregationType,
      value: calculateAggregationValue(dataView, aggregationInfo),
    }
  })
}

function calculateAggregationValue(dataView: IDataView, aggregationInfo: IAggregationInfo){
  const properties = getProperties(dataView);
  const property = properties.find(prop => prop.id === aggregationInfo.ColumnName)!
  const values = dataView.dataTable.rows
    .map(row => dataView.dataTable.getCellValue(row, property) as number);

  if(values.length === 0){
    return 0;
  }
  switch (aggregationInfo.AggregationType) {
    case AggregationType.SUM:
      return values.reduce((a, b) => a + b, 0);
    case AggregationType.AVG:
      return values.reduce((a, b) => a + b, 0) / values.length;
    case AggregationType.MAX:
      return Math.max(...values);
    case AggregationType.MIN:
      return Math.min(...values);
    case AggregationType.COUNT:
      return values.length;
  }
}
