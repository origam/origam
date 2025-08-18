/*
Copyright 2005 - 2023 Advantage Solutions, s. r. o.

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

import { CellAlignment } from "gui/Components/ScreenElements/Table/TableRendering/cells/cellAlignment";

// Makes sure the editor alignment will be the same as the table cell alignment.
// Needed on columns where the alignment can be set in the model.
export function resolveCellAlignment(
  customStyle: { [p: string]: string } | undefined,
  isFirstColumn: boolean,
  type: string)
  : { [key: string]: string } {
  let cellAlignment = new CellAlignment(isFirstColumn, type, customStyle);
  const style = customStyle ? Object.assign({}, customStyle) : {};
  style["paddingRight"] = cellAlignment.paddingRight - 1 + "px";
  style["paddingLeft"] = cellAlignment.paddingLeft + "px";
  style["textAlign"] = cellAlignment.alignment;
  return style;
}