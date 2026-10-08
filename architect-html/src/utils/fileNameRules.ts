/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

const RESERVED_DEVICE_NAME = /^(con|prn|aux|nul|com[1-9]|lpt[1-9])(\.|$)/i;
const INVALID_FILE_NAME_CHARS = /[\\/:*?"<>|]/;

export function hasInvalidFileNameChars(name: string): boolean {
  if (INVALID_FILE_NAME_CHARS.test(name)) {
    return true;
  }
  for (const char of name) {
    if (char.charCodeAt(0) < 0x20) {
      return true;
    }
  }
  return false;
}

export function isReservedOrUnsafeFileName(name: string): boolean {
  if (name === '.' || name === '..') {
    return true;
  }
  if (name.endsWith('.')) {
    return true;
  }
  return RESERVED_DEVICE_NAME.test(name);
}
