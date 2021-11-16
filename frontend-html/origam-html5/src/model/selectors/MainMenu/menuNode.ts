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

import { T } from "utils/translation";

export function getAllParents(node: any) {
  const parents: any[] = [];
  let cn = node;
  while (cn !== undefined) {
    parents.push(cn);
    cn = cn.parent;
  }
  parents.reverse();
  return parents.slice(2); // Strip out root and Menu node
}

export function getPath(node: any) {
  const nodeLabels = getAllParents(node).map(node => node.attributes.label);
  nodeLabels.unshift(T("Menu", "menu"));
  return nodeLabels.join("/")
}
