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

export function ValueBox<T>() {
  let _active = false;
  let _value: T | undefined;

  function get() {
    if (_active) return _value!;
    throw new Error("Value not set.");
  }

  function set(value: T) {
    if (_active) throw new Error("Value already set.");
    _value = value;
    _active = true;
  }

  function clear() {
    _value = undefined;
    _active = false;
  }

  function isSet() {
    return _active;
  }

  get.isSet = isSet;
  get.get = get;
  get.set = set;
  get.clear = clear;

  return get;
}
