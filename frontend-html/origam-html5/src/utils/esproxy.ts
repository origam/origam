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

export interface IParented<TParent> {
  parent?: TParent;
}

export function proxyEnrich<TAdd, TTarget extends object & IParented<TAdd>>(
  target: TTarget
) {
  return new Proxy<TTarget>(target, {
    get(obj, prop) {
      return prop in obj ? (obj as any)[prop] : (obj.parent as any)[prop];
    },
    set(obj, prop, newVal) {
      if (prop in obj) {
        (obj as any)[prop] = newVal;
      } else {
        (obj.parent as any)[prop] = newVal;
      }
      return true;
    }
  }) as TTarget & TAdd;
}