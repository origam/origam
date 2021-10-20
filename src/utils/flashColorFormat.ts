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

export function flashColor2htmlColor(flashColor: number) {
  if (flashColor === 0 || flashColor === null || flashColor === undefined) {
    return undefined;
  }
  return (
    "#" +
    (flashColor < 0 ? flashColor + 0xffffffff + 1 : flashColor)
      .toString(16)
      .slice(-6)
      .toUpperCase()
      .padStart(6, "0")
  );
}

export function htmlColor2FlashColor(htmlColor: string | undefined | null) {
  if (htmlColor === undefined || htmlColor === null) {
    return htmlColor;
  }
  if (htmlColor.startsWith("#")) {
    htmlColor = htmlColor.slice(1);
  }
  const flashColor = parseInt(htmlColor, 16);
  return flashColor;
}
