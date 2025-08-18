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

import { clickSubscriptions, mouseMoveSubscriptions, mouseOverSubscriptions, } from "./renderingValues";
import { IClickSubsItem, IMouseMoveSubsItem, IMouseOverSubsItem } from "./types";

export function onClick(item: IClickSubsItem) {
  clickSubscriptions().push(item);
}

export async function handleTableClick(
  event: any,
  canvasX: number,
  canvasY: number,
  scrollLeft: number,
  scrollTop: number,
  clickSubscriptions: IClickSubsItem[]
) {
  let handled = false;
  for (let h of clickSubscriptions) {
    if (h.x <= canvasX && h.x + h.w >= canvasX && h.y <= canvasY && h.y + h.h >= canvasY) {
      await h.handler(event, canvasX, canvasY, canvasX, canvasY);
      handled = true;
      break;
    }
  }
  return handled;
}

export function onMouseMove(item: IMouseMoveSubsItem) {
  mouseMoveSubscriptions().push(item);
}

export function handleTableMouseMove(
  event: any,
  canvasX: number,
  canvasY: number,
  scrollLeft: number,
  scrollTop: number,
  mouseMoveSubscriptions: IClickSubsItem[]
) {
  let handled = false;
  for (let h of mouseMoveSubscriptions) {
    if (h.x <= canvasX && h.x + h.w >= canvasX && h.y <= canvasY && h.y + h.h >= canvasY) {
      h.handler(event, canvasX, canvasY, canvasX, canvasY);
      handled = true;
      break;
    }
  }
  return {handled};
}

export function onMouseOver(item: IMouseOverSubsItem) {
  mouseOverSubscriptions().push(item);
}

export function getTooltip(
  canvasX: number,
  canvasY: number,
  mouseOverSubscriptions: IMouseOverSubsItem[]
) {
  for (let h of mouseOverSubscriptions) {
    if (h.x <= canvasX && h.x + h.w >= canvasX && h.y <= canvasY && h.y + h.h >= canvasY) {
      return h.tooltipGetter(canvasX, canvasY, canvasX, canvasY);
    }
  }
  return undefined;
}
