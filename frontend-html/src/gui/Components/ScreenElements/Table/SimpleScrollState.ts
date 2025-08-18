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

import { IScrollState } from "./types";
import { action, observable } from "mobx";

export class SimpleScrollState implements IScrollState {
  scrollToFunction: ((coords: { scrollLeft?: number; scrollTop?: number }) => void) | undefined;

  constructor(scrollTop: number, scrollLeft: number) {
    this.scrollTop = scrollTop;
    this.scrollLeft = scrollLeft;
  }

  scrollTo(coords: { scrollLeft?: number; scrollTop?: number }) {
    if (this.scrollToFunction) {
      this.scrollToFunction(coords);
    }
  }

  scrollBy(coords: {deltaLeft?: number; deltaTop?: number}) {
    this.scrollTo({
      scrollLeft: coords.deltaLeft !== undefined ? this.scrollLeft + coords.deltaLeft : undefined,
      scrollTop: coords.deltaTop !== undefined ? this.scrollTop + coords.deltaTop : undefined
    })
  }

  @observable scrollTop = 0;
  @observable scrollLeft = 0;

  @action.bound
  setScrollOffset(event: any, scrollTop: number, scrollLeft: number): void {
    // console.log("scroll event: ", scrollTop, scrollLeft);
    this.scrollTop = scrollTop;
    this.scrollLeft = scrollLeft;
  }
}
