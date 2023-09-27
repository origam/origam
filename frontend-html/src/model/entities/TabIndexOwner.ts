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

export interface ITabIndexOwner {
  tabIndex: TabIndex;
}

export class TabIndex {

  private _fractions: number[];

  get fractions(){
    return this._fractions;
  }

  public static Min = new TabIndex("-1");
  public static Max = new TabIndex("1000000");

  constructor(tabIndex: string | undefined) {
    this._fractions = this.getTabIndexFractions(tabIndex);
  }

  private getTabIndexFractions(tabIndex: string | undefined): number[] {
    if (tabIndex) {
      return tabIndex
        .split(".")
        .filter((x) => x !== "")
        .map((x) => parseInt(x));
    }
    return TabIndex.Max.fractions;
  }
}

// TabIndex is a string separated by decimal points for example: 13, 14.0, 14.2, 14.15
// The "fractions" have to be compared separately because 14.15 is greater than 14.2
// Comparison as numbers would give different results
export function compareTabIndexOwners(x: ITabIndexOwner, y: ITabIndexOwner) {
  return compareFraction(x, y, 0);
}

function compareFraction(
  x: ITabIndexOwner,
  y: ITabIndexOwner,
  fractionIndex: number
): number {
  if (has(x, fractionIndex) && !has(y, fractionIndex)) {
    return 1;
  }
  if (!has(x, fractionIndex) && has(y, fractionIndex)) {
    return -1;
  }
  if (!has(x, fractionIndex) && !has(y, fractionIndex)) {
    return 0;
  }

  const fractionDifference = x.tabIndex.fractions[fractionIndex] - y.tabIndex.fractions[fractionIndex];
  if (fractionDifference !== 0) {
    return fractionDifference;
  }

  return compareFraction(x, y, fractionIndex + 1);
}

function has(tabIndexOwner: ITabIndexOwner, fractionIndex: number) {
  return tabIndexOwner.tabIndex.fractions.length - 1 >= fractionIndex;
}
