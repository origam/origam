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

export function flf2mof(flf: string) {
  return flf
    .replace(/YYYYY/g, "YYYY")
    .replace(/E/g, "d")
    .replace(/A/g, "a")
    .replace(/H/g, "k")
    .replace(/J/g, "H")
    .replace(/K/g, "h")
    .replace(/N/g, "m")
    .replace(/S/g, "s")
    .replace(/Q/g, "S");
}
