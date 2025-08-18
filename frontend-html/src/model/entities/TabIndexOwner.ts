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

  private readonly _fractions: number[];
  private static readonly MaxFraction = 1000000;
  private static readonly MinFraction = -1;

  get fractions(){
    return this._fractions;
  }

  public static Min = new TabIndex([TabIndex.MinFraction]);
  public static Max = new TabIndex([TabIndex.MaxFraction]);

  public static create(tabIndex: string | undefined) {
    const fractions = getTabIndexFractions(tabIndex);
    return new TabIndex(fractions);
  }
  constructor(fractions: number[]) {
    this._fractions = fractions;
  }

  public toString(){
    return this.fractions.join(".");
  }

  public isMax(){
    return this.fractions.length === 1 && this.fractions[0] === TabIndex.MaxFraction;
  }
}

function getTabIndexFractions(tabIndex: string | undefined): number[] {
  if (tabIndex) {
    return tabIndex
      .split(".")
      .filter((x) => x !== "")
      .map((x) => parseInt(x));
  }
  return TabIndex.Max.fractions;
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
  if (!tabIndexOwner.tabIndex.fractions) {
    return false;
  }
  return tabIndexOwner.tabIndex.fractions.length - 1 >= fractionIndex;
}

export function getCommonTabIndex(owners: ITabIndexOwner[]){
  const commonFractions: number[] = [];
  for (let i = 0; i < 100; i++) {
    const uniqueFractions:Set<number> = new Set();
    for (let owner of owners) {
      if (i <= owner.tabIndex.fractions.length - 1 && !owner.tabIndex.isMax()) {
        uniqueFractions.add(owner.tabIndex.fractions[i]);
      }
    }
    if(uniqueFractions.size > 1 || uniqueFractions.size === 0){
      return commonFractions.length === 0
          ? TabIndex.Max
          : new TabIndex(commonFractions);
    }
    else{
      let fraction = Array.from(uniqueFractions.values())[0];
      commonFractions.push(fraction);
    }
  }
  throw new Error("Could not find tabIndex of : " + owners);
}
