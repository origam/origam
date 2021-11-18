import React from "react";
import cx from "classnames";
import { PropsWithChildren } from "react";

export function bodyCellClass(rowIndex: number, selected: boolean, withCursor: boolean) {
  return cx("cell", rowIndex % 2 ? "ord2" : "ord1", { withCursor, selected });
}

export const CtxCell = React.createContext({ visibleRowIndex: 0, visibleColumnIndex: 0 });
