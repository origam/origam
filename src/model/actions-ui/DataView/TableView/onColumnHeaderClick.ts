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

import {getOrderingConfiguration} from "model/selectors/DataView/getOrderingConfiguration";
import {flow} from "mobx";
import {handleError} from "model/actions/handleError";
import { getProperties } from "model/selectors/DataView/getProperties";

export function onColumnHeaderClick(ctx: any) {
  return flow(function* onColumnHeaderClick(event: any, column: string) {
    try {
      const property = getProperties(ctx).find(prop => prop.id === column);
      if(property?.column === "Blob" || property?.column === "TagInput"){
        return;
      }
      if (event.ctrlKey || event.metaKey) {
        getOrderingConfiguration(ctx).addOrdering(column);
      } else {
        getOrderingConfiguration(ctx).setOrdering(column);
      }
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
