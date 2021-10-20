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

import React, { PropsWithChildren, useContext } from "react";

import { Rect } from "react-measure";
import { CtxDropdownBodyRect, CtxDropdownCtrlRect } from "./DropdownCommon";

export function DropdownLayoutBody(
  props: PropsWithChildren<{ render: () => React.ReactNode, minSideMargin: number }>
) {

  const rectBody = useContext(CtxDropdownBodyRect);
  const rectCtrl = useContext(CtxDropdownCtrlRect);

  function rectBodyComputed(): Rect {
    const rect = {top: undefined, left: undefined, width: undefined, height: undefined} as any;
    if (rectCtrl.bottom! + rectBody.height! > window.innerHeight - 50) {
      rect.top = rectCtrl.top! - rectBody.height!;
    } else {
      rect.top = rectCtrl.bottom;
    }

    if (rectCtrl.left! + rectBody.width! > window.innerWidth - props.minSideMargin) {
      rect.left = Math.max(props.minSideMargin, window.innerWidth - rectBody.width! - props.minSideMargin);
    } else {
      rect.left = rectCtrl.left;
    }

    rect.width = Math.min(Math.max(rectCtrl.width!, rectBody.width!), window.innerWidth - 2 * props.minSideMargin);
    rect.height = rectBody.height;

    return rect;
  }

  const rectBodyCom = rectBodyComputed();

  return (
    <div
      style={{
        position: "fixed",
        // overflow: "hidden",
        zIndex: 2000,
        top: rectBodyCom.top,
        left: rectBodyCom.left,
        width: rectBodyCom.width,
        height: rectBodyCom.height,
      }}
    >
      {props.render()}
    </div>
  );
}

DropdownLayoutBody.defaultProps = {
  render: () => null,
  minSideMargin: 50
};