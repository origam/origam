import React, { PropsWithChildren, useContext } from "react";

import { Rect } from "react-measure";
import { CtxDropdownBodyRect, CtxDropdownCtrlRect } from "./DropdownCommon";

export function DropdownLayoutBody(
  props: PropsWithChildren<{ render: () => React.ReactNode }>
) {

  const rectBody = useContext(CtxDropdownBodyRect);
  const rectCtrl = useContext(CtxDropdownCtrlRect);

  function rectBodyComputed(): Rect {
    const rect = { top: undefined, left: undefined, width: undefined, height: undefined } as any;
    if (rectCtrl.bottom! + rectBody.height! > window.innerHeight - 50) {
      rect.top = rectCtrl.top! - rectBody.height!;
    } else {
      rect.top = rectCtrl.bottom;
    }

    if (rectCtrl.left! + rectBody.width! > window.innerWidth - 50) {
      rect.left = Math.max(50, window.innerWidth - rectBody.width! - 50);
    } else {
      rect.left = rectCtrl.left;
    }

    rect.width = Math.min(Math.max(rectCtrl.width!, rectBody.width!), window.innerWidth - 100);
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