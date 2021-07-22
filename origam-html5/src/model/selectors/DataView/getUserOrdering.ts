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

import { getOrderingConfiguration } from "./getOrderingConfiguration";
import { getDataStructureEntityId } from "./getDataStructureEntityId";
import { getDataView } from "./getDataView";
import { IOrderByDirection } from "model/entities/types/IOrderingConfiguration";

export function getUserOrdering(ctx: any) {
  const dataView =  getDataView(ctx);
  const orderingConfiguration = getOrderingConfiguration(dataView);
  const defaultOrderings = orderingConfiguration.getDefaultOrderings();
  if(defaultOrderings.length === 0){
    const dataStructureEntityId = getDataStructureEntityId(dataView);
    throw new Error(`Cannot infinitely scroll on dataStructureEntity: ${dataStructureEntityId} because it has no default ordering on the displayed form.`)
  }
  return orderingConfiguration.userOrderings.length === 0
    ? defaultOrderings
    : orderingConfiguration.userOrderings
      .filter(ordering => ordering.direction !== IOrderByDirection.NONE);
}