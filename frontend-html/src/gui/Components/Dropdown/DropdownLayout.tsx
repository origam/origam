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

import Measure, { BoundingRect, ContentRect } from "react-measure";
import React, { useEffect, useRef, useState } from "react";
import ReactDOM from "react-dom";
import { CtxDropdownBodyRect, CtxDropdownCtrlRect, CtxDropdownRefBody, CtxDropdownRefCtrl, } from "./DropdownCommon";
import _ from "lodash";

export function DropdownLayout(props: {
  isDropped: boolean;
  renderCtrl: () => React.ReactNode;
  renderDropdown: () => React.ReactNode;
  onDropupRequest?: () => void;
}) {
  function handleCtrlBlockResize(contentRect: ContentRect) {
    const {top, left, width, height, bottom, right} = contentRect.bounds!;
    setRectCtrl({
      top,
      left,
      width,
      height,
      bottom,
      right,
    });
  }

  function handleBodyBlockResize(contentRect: ContentRect) {
    const {top, left, width, height, bottom, right} = contentRect.bounds!;
    setRectBody({
      top,
      left,
      width,
      height,
      bottom,
      right,
    });
  }

  const refMeasureCtrl = useRef<Measure>(null);
  const refMeasureBody = useRef<Measure>(null);

  const [rectCtrl, setRectCtrl] = useState<BoundingRect>({
    top: window.screen.height + 50,
    left: window.screen.width + 50,
    width: 0,
    height: 0,
    bottom: 0,
    right: 0,
  });

  const [rectBody, setRectBody] = useState<BoundingRect>({
    top: window.screen.height + 50,
    left: window.screen.width + 50,
    width: 0,
    height: 0,
    bottom: 0,
    right: 0,
  });

  const reMeasure = () => {
    if (refMeasureCtrl.current) {
      (refMeasureCtrl.current as any).measure();
    }
    if (refMeasureBody.current) {
      (refMeasureBody.current as any).measure();
    }
  };

  const elmDropdownPortal = document.getElementById("dropdown-portal")!;

  useEffect(() => {
    let intervalHandle: any;
    const handleScroll = _.throttle((event: any) => {
      if (
        elmDropdownPortal &&
        !elmDropdownPortal.contains(event.target) &&
        event.target.tagName !== "INPUT" && // the scroll event is fired in the focused INPUT after clicking the drop icon in firefox. Preventing the dropdown from being shown.
        props.isDropped)
      {
        console.log(event);
        props.onDropupRequest?.();
      }
    }, 100);
    const handleMouse = _.throttle((event: any) => {
      if (props.isDropped) reMeasure();
    }, 100);
    if (props.isDropped) {
      reMeasure();
      intervalHandle = setInterval(() => {
        reMeasure();
      }, 3000);
    }
    window.addEventListener("scroll", handleScroll, true);
    window.addEventListener("mousedown", handleMouse, true);
    window.addEventListener("mouseup", handleMouse, true);
    return () => {
      window.removeEventListener("scroll", handleScroll, true);
      window.removeEventListener("mousedown", handleMouse, true);
      window.removeEventListener("mouseup", handleMouse, true);
      clearInterval(intervalHandle);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [props.isDropped, props.onDropupRequest, elmDropdownPortal]);


  return (
    <>
      <CtxDropdownBodyRect.Provider value={rectBody}>
        <CtxDropdownCtrlRect.Provider value={rectCtrl}>
          <Measure ref={refMeasureCtrl} bounds={true} onResize={handleCtrlBlockResize}>
            {({measureRef}) => (
              <CtxDropdownRefCtrl.Provider value={measureRef}>
                {props.renderCtrl()}
              </CtxDropdownRefCtrl.Provider>
            )}
          </Measure>
          {props.isDropped &&
          ReactDOM.createPortal(
            <Measure ref={refMeasureBody} bounds={true} onResize={handleBodyBlockResize}>
              {({measureRef}) => (
                <CtxDropdownRefBody.Provider value={measureRef}>
                  {props.renderDropdown()}
                </CtxDropdownRefBody.Provider>
              )}
            </Measure>,
            elmDropdownPortal
          )}
        </CtxDropdownCtrlRect.Provider>
      </CtxDropdownBodyRect.Provider>
    </>
  );
}
