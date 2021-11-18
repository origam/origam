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

import { getIdent } from "utils/common";

export function find(xmlObj: any, pred: (node: any) => boolean) {
  function recursive(n: any) {
    if (xmlObj !== n && pred(n)) {
      result.push(n);
    }
    if (n.elements) {
      for (let chn of n.elements) {
        recursive(chn);
      }
    }
  }

  const result: any[] = [];
  recursive(xmlObj);
  return result;
}

export function findStopping(xmlObj: any, pred: (node: any) => boolean) {
  function recursive(n: any) {
    if (xmlObj !== n && pred(n)) {
      result.push(n);
    } else if (n.elements) {
      for (let chn of n.elements) {
        recursive(chn);
      }
    }
  }

  const result: any[] = [];
  recursive(xmlObj);
  return result;
}

export function assignIIds(xmlTree: any) {
  function recursive(node: any) {
    node.$iid = getIdent();
    if (node.elements) {
      for (let e of node.elements) recursive(e);
    }
  }

  recursive(xmlTree);
}
