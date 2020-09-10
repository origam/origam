import { clickSubscriptions } from "./renderingValues";
import { IClickSubsItem } from "./types";

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
