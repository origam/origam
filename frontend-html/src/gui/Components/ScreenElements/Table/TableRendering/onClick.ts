import {
  clickSubscriptions,
  mouseOverSubscriptions,
  mouseMoveSubscriptions,
} from "./renderingValues";
import { IClickSubsItem, IMouseOverSubsItem } from "./types";

export function onClick(item: IClickSubsItem) {
  clickSubscriptions().push(item);
}

export function handleTableClick(
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
      h.handler(event, canvasX, canvasY, canvasX, canvasY);
      handled = true;
      break;
    }
  }
  return { handled };
}

export function onMouseMove(item: IClickSubsItem) {
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
  return { handled };
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
      return h.toolTipGetter(canvasX, canvasY, canvasX, canvasY);
    }
  }
  return undefined;
}
