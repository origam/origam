import {
  ICellRenderer,
  ICellRendererArgs,
  T$4,
  T$2,
  ICellRect,
  ICellInfo
} from "./types";
import { trcpr } from "../utils/canvas";

export function createGridCellRenderer({
  onClick
}: {
  onClick: (event: any, cellRect: ICellRect, cellInfo: ICellInfo) => void;
}): ICellRenderer {
  return function gridCellRenderer({
    ctx,
    columnIndex,
    rowIndex,
    cellDimensions,
    events
  }: ICellRendererArgs) {
    ctx.fillStyle = rowIndex % 2 === 0 ? "#ffffff" : "#efefef";
    ctx.fillRect(
      ...(trcpr(0, 0, cellDimensions.width, cellDimensions.height) as T$4)
    );
    ctx.fillStyle = "black";
    let text;
    /*
  if (columnIndex === 0) {
    text = dataTable.records[rowIndex].name;
  } else if (columnIndex === 1) {
    text = moment(dataTable.records[rowIndex].birth_date).format(
      "DD.MM.YYYY"
    );
  } else if (columnIndex === 2) {
    text = dataTable.records[rowIndex].favorite_color;
  } else if (columnIndex === 3) {
    text = dataTable.records[rowIndex].id;
  } else {
    text = `Cell ${columnIndex};${rowIndex}`;
  }*/
    text = `Cell ${columnIndex};${rowIndex}`;
    ctx.fillText(text, ...(trcpr(15, 15) as T$2));

    events.onClick((event: any, cellRect: ICellRect, cellInfo: ICellInfo) => {
      // console.log(cellInfo.rowIndex, cellInfo.columnIndex);
      onClick(event, cellRect, cellInfo)
    });
  };
}
