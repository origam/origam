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

import { IProperty } from "model/entities/types/IProperty";
import { getDataSourceFieldByName } from "../DataSources/getDataSourceFieldByName";
import { getDataTable } from "./getDataTable";

export function getFieldErrorMessage(ctx: any) {
  return function getFieldErrorMessage(row: any[], property: IProperty) {
    if (row) {
      const dataTable = getDataTable(ctx);
      const dsFieldErrors = getDataSourceFieldByName(ctx, "__Errors");
      const errors = dsFieldErrors
        ? dataTable.getCellValueByDataSourceField(row, dsFieldErrors)
        : null;

      const errMap: Map<number, string> | undefined = errors
        ? new Map(
            Object.entries<string>(
              errors.fieldErrors
            ).map(([dsIndexStr, errMsg]: [string, string]) => [parseInt(dsIndexStr, 10), errMsg])
          )
        : undefined;

      const errMsg = dsFieldErrors && errMap ? errMap.get(property.dataSourceIndex) : undefined;
      return errMsg;
    }
  };
}
