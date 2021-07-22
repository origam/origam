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

import {currentRowCellsDimensions} from "./currentRowCells";
import {viewportLeft, viewportRight} from "./renderingValues";

export function firstDrawableColumnIndex() {
  const dimensions = currentRowCellsDimensions();
  const wpLeft = viewportLeft();
  for(let i = 0; i < dimensions.length; i++) {
    if(dimensions[i].right >= wpLeft) return i
  }
}

export function lastDrawableColumnIndex() {
  const dimensions = currentRowCellsDimensions();
  const wpRight = viewportRight();
  for(let i = dimensions.length - 1; i >= 0 ; i--) {
    if(dimensions[i].left <= wpRight) return i
  }
}