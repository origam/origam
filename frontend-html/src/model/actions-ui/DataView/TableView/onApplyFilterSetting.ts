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

import { flow } from "mobx";
import { getProperty } from "model/selectors/DataView/getProperty";
import { getFilterConfiguration } from "../../../selectors/DataView/getFilterConfiguration";
import { handleError } from "model/actions/handleError";
import { IFilterSetting } from "../../../entities/types/IFilterSetting";

export function onApplyFilterSetting(ctx: any) {
  const prop = getProperty(ctx);
  return flow(function*onApplyFilterSetting(setting: IFilterSetting) {
    try {
      getFilterConfiguration(ctx).setFilter(
        {propertyId: prop.id, dataType: prop.column, setting});
    } catch (e) {
      yield*handleError(ctx)(e);
      throw e;
    }
  });
}
