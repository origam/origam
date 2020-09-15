import Measure, { BoundingRect, ContentRect } from "react-measure";
import React, { useRef, useState, useEffect } from "react";
import ReactDOM from "react-dom";
import {
  CtxDropdownRefBody,
  CtxDropdownRefCtrl,
  CtxDropdownBodyRect,
  CtxDropdownCtrlRect,
} from "./DropdownCommon";
import _ from "lodash";

export function DropdownLayout(props: {
  isDropped: boolean;
  renderCtrl: () => React.ReactNode;
  renderDropdown: () => React.ReactNode;
}) {
  function handleCtrlBlockResize(contentRect: ContentRect) {
    const { top, left, width, height, bottom, right } = contentRect.bounds!;
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
    const { top, left, width, height, bottom, right } = contentRect.bounds!;
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

  useEffect(() => {
    const handleScroll = _.throttle((event: any) => {
      if (props.isDropped) reMeasure();
    }, 100);
    const handleMouse = _.throttle((event: any) => {
      if (props.isDropped) reMeasure();
    }, 100);
    if (props.isDropped) {
      reMeasure();
    }
    window.addEventListener("scroll", handleScroll, true);
    window.addEventListener("mousedown", handleMouse, true);
    window.addEventListener("mouseup", handleMouse, true);
    return () => {
      window.removeEventListener("scroll", handleScroll, true);
      window.removeEventListener("mousedown", handleMouse, true);
      window.removeEventListener("mouseup", handleMouse, true);
    };
  }, [props.isDropped]);

  return (
    <>
      <CtxDropdownBodyRect.Provider value={rectBody}>
        <CtxDropdownCtrlRect.Provider value={rectCtrl}>
          <Measure ref={refMeasureCtrl} bounds={true} onResize={handleCtrlBlockResize}>
            {({ measureRef }) => (
              <CtxDropdownRefCtrl.Provider value={measureRef}>
                {props.renderCtrl()}
              </CtxDropdownRefCtrl.Provider>
            )}
          </Measure>
          {props.isDropped &&
            ReactDOM.createPortal(
              <Measure ref={refMeasureBody} bounds={true} onResize={handleBodyBlockResize}>
                {({ measureRef }) => (
                  <CtxDropdownRefBody.Provider value={measureRef}>
                    {props.renderDropdown()}
                  </CtxDropdownRefBody.Provider>
                )}
              </Measure>,
              document.getElementById("dropdown-portal")!
            )}
        </CtxDropdownCtrlRect.Provider>
      </CtxDropdownBodyRect.Provider>
    </>
  );
}
