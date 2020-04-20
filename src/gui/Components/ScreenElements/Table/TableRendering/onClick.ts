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
  
  const worldX = canvasX;// + scrollLeft;
  const worldY = canvasY;// + scrollTop;
  for (let h of clickSubscriptions) {
    if (h.x <= worldX && h.x + h.w >= worldX && h.y <= worldY && h.y + h.h >= worldY) {
      h.handler(event, worldX, worldY, canvasX, canvasY);
    }
  }
}
