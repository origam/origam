import {currentRowCellsDimensions} from "./currentRowCells";
import {viewportLeft, viewportRight} from "./renderingValues";

export function firstDrawableColumnIndex() {
  const dimensions = currentRowCellsDimensions();
  const wpLeft = viewportLeft();
  for(let i = 0; i < dimensions.length; i++) {
    if(dimensions[i].right >= wpLeft) return i
  }
}

export function lastDrawableColumnIndex() {
  const dimensions = currentRowCellsDimensions();
  const wpRight = viewportRight();
  for(let i = dimensions.length - 1; i >= 0 ; i--) {
    if(dimensions[i].left <= wpRight) return i
  }
}