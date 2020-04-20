import { rowHeight, viewportBottom, viewportTop } from "./renderingValues";


export function firstDrawableRowIndex() {
  return Math.floor(viewportTop() / rowHeight());
}

export function lastDrawableRowIndex() {
  return Math.ceil(viewportBottom() / rowHeight());
}