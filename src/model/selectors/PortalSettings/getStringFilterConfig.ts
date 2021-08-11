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

import {IFilterConfig} from "../../entities/types/IFilterConfig";
import {getWorkbenchLifecycle} from "../getWorkbenchLifecycle";
import {latinize} from "utils/string";

function getStringFilterConfig(ctx: any): IFilterConfig {
  return getWorkbenchLifecycle(ctx).portalSettings?.filterConfig ?? {
    caseSensitive: false,
    accentSensitive: true
  };
}

export function prepareForFilter(ctx: any, text: string | undefined | null){
  if(text === undefined || text === null){
    return text;
  }
  const filterConfig = getStringFilterConfig(ctx);
  if (!filterConfig.caseSensitive) {
     text = text.toLowerCase();
  }
  if (!filterConfig.accentSensitive) {
    text = latinize(text);
  }
  return text;
}

export function prepareAnyForFilter(ctx: any, value: any): any {

  if (typeof value === 'string' || value instanceof String){
    return prepareForFilter(ctx, value as string);
  }
  if(Array.isArray(value)){
    return value.map(val =>  prepareAnyForFilter(ctx, val))
  }
  return value;
}

