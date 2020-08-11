import React from "react";
import { BoundingRect } from "react-measure";


export const CtxDropdownBodyRect = React.createContext<BoundingRect>({
  top: 0,
  left: 0,
  height: 0,
  width: 0,
  right: 0,
  bottom: 0,
});

export const CtxDropdownCtrlRect = React.createContext<BoundingRect>({
  top: 0,
  left: 0,
  height: 0,
  width: 0,
  right: 0,
  bottom: 0,
});

export const CtxDropdownRefBody = React.createContext<any>(undefined);

export const CtxDropdownRefCtrl = React.createContext<any>(undefined);